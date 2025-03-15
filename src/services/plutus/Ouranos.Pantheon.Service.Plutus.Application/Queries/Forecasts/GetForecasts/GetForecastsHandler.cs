using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetForecasts;

public sealed class GetForecastsHandler : QueryHandler<GetForecastsInput, WrapperResponse<List<Forecast>>>
{
    private readonly IRepository<Forecast> _forecastRepository;
    private readonly IGetForecastPredictions _getForecastPredictions;
    private readonly ILogger<GetForecastsHandler> _logger;

    public GetForecastsHandler(
        ILogger<GetForecastsHandler> logger,
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
        GetForecastsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get forecasts query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var predictions = await _getForecastPredictions.GetPredictionsAsync(
            query.HistoricalData.Select(x => x.Value).ToList(),
            cancellationToken
        );

        var forecasts = predictions
            .Select(
                (p, i) =>
                {
                    var symbolId = query.HistoricalData.Keys.ElementAt(i);
                    var symbol = query.Symbols.First(s => s.Id == symbolId);

                    return new Forecast(
                        _forecastRepository.CreateId(),
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