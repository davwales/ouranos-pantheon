using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketForecast;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetConstructedForecasts;

public sealed class GetConstructedForecastsHandler
    : QueryHandler<GetConstructedForecastsInput, WrapperResponse<List<Forecast>>>
{
    private readonly IRepository<Forecast> _forecastRepository;
    private readonly IGetForecastPredictions _getForecastPredictions;
    private readonly ILogger<GetMarketForecastHandler> _logger;

    public GetConstructedForecastsHandler(
        ILogger<GetMarketForecastHandler> logger,
        IGetForecastPredictions getForecastPredictions,
        IRepository<Forecast> forecastRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(getForecastPredictions);
        Guard.Against.Null(forecastRepository);

        _logger = logger;
        _getForecastPredictions = getForecastPredictions;
        _forecastRepository = forecastRepository;
    }

    public override async Task<WrapperResponse<List<Forecast>>> Handle(
        GetConstructedForecastsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get constructed forecasts query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var predictions = await _getForecastPredictions.GetPredictionsAsync(
            query.HistoricalData.Select(x => x.Value).ToList(),
            cancellationToken
        );

        var forecasts = predictions
            .Select((p, i) =>
                {
                    var symbolId = query.HistoricalData.Keys.ElementAt(i);
                    var symbol = query.Symbols.First(s => s.Id == symbolId);

                    return new Forecast(
                        _forecastRepository.CreateId(),
                        symbol.MarketId,
                        symbolId,
                        symbol.Name,
                        symbol.Subcode,
                        query.HistoricalData.Values.ElementAt(i).Last(),
                        p
                    );
                }
            )
            .ToList();

        var response = new WrapperResponse<List<Forecast>>(forecasts);

        _logger.LogDebug("Successfully handled get forecasts query.");
        return response;
    }
}