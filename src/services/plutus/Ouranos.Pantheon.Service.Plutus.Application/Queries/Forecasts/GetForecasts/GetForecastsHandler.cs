using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetForecasts;

public sealed class GetForecastsHandler
    : QueryHandler<GetForecastsInput, WrapperResponse<IQueryable<GetForecastsResponse>>>
{
    private readonly IRepository<Forecast> _forecastRepository;
    private readonly ILogger<GetForecastsHandler> _logger;

    public GetForecastsHandler(
        ILogger<GetForecastsHandler> logger,
        IRepository<Forecast> forecastRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(forecastRepository);

        _logger = logger;
        _forecastRepository = forecastRepository;
    }

    public override async Task<WrapperResponse<IQueryable<GetForecastsResponse>>> Handle(
        GetForecastsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get forecasts query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var forecastsQuery = _forecastRepository
            .AsQueryable(cancellationToken)
            .Where(f => f.Predictions.Count >= 7)
            .Select(
                f => new
                {
                    f.Id,
                    f.MarketId,
                    f.SymbolId,
                    f.SymbolName,
                    f.SymbolSubcode,
                    f.Latest,
                    Predictions = f.Predictions.Select(
                        p => new GetForecastsPredictionResponse(
                            p.AveragePrice,
                            p.MinPrice,
                            p.MaxPrice,
                            p.Volume,
                            p.AveragePrice - f.Latest.AveragePrice,
                            p.MinPrice - f.Latest.MinPrice,
                            p.MaxPrice - f.Latest.MaxPrice,
                            p.Volume - f.Latest.Volume,
                            p.AveragePrice * p.Volume - f.Latest.AveragePrice * f.Latest.Volume
                        )
                    )
                }
            )
            .Select(
                x => new GetForecastsResponse(
                    x.Id,
                    x.MarketId,
                    x.SymbolId,
                    x.SymbolName,
                    x.SymbolSubcode,
                    x.Latest,
                    x.Predictions.ElementAt(0),
                    x.Predictions.ElementAt(1),
                    x.Predictions.ElementAt(2),
                    x.Predictions.ElementAt(3),
                    x.Predictions.ElementAt(4),
                    x.Predictions.ElementAt(5),
                    x.Predictions.ElementAt(6)
                )
            );

        var response = new WrapperResponse<IQueryable<GetForecastsResponse>>(forecastsQuery);

        _logger.LogDebug("Successfully handled get forecasts query.");
        return await Task.FromResult(response);
    }
}