using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;

public sealed class GetMarketForecastHandler
    : QueryHandler<GetMarketForecastInput, WrapperResponse<IQueryable<GetMarketForecastResponse>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetMarketForecastHandler> _logger;

    public GetMarketForecastHandler(
        ILogger<GetMarketForecastHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<GetMarketForecastResponse>>> Handle(
        GetMarketForecastInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get market forecast query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _dbContext.Markets.FirstOrDefaultAsync(m => m.Id == query.MarketId, cancellationToken);

        Guard.Against.NotFound(query.MarketId, market);

        var flatTax = market.Taxes.Flat ?? new FlatTax(0, 0, 0);

        var forecastsQuery = _dbContext.Forecasts
            .Where(f => f.MarketId == query.MarketId && f.Predictions.Count >= 7)
            .Select(f => new
            {
                f.Id,
                f.MarketId,
                f.SymbolId,
                SymbolName = f.Symbol.Name,
                SymbolSubcode = f.Symbol.Subcode,
                f.Latest,
                Predictions = f.Predictions.Select(p => new
                {
                    p.AveragePrice,
                    p.MaxPrice,
                    p.MinPrice,
                    p.Volume,
                    Margin = p.AveragePrice - (
                            p.AveragePrice * flatTax.Rate > 0
                                ? 0
                                : p.AveragePrice * flatTax.Rate
                        ) - f.Latest.AveragePrice
                }
                    )
            }
            )
            .Select(f => new
            {
                f.Id,
                f.MarketId,
                f.SymbolId,
                f.SymbolName,
                f.SymbolSubcode,
                f.Latest,
                Predictions = f.Predictions.Select(p => new GetMarketForecastPredictionResponse(
                        p.AveragePrice,
                        p.MinPrice,
                        p.MaxPrice,
                        p.Volume,
                        p.Margin,
                        p.Margin * p.Volume,
                        p.AveragePrice - f.Latest.AveragePrice,
                        p.MinPrice - f.Latest.MinPrice,
                        p.MaxPrice - f.Latest.MaxPrice,
                        p.Volume - f.Latest.Volume,
                        p.AveragePrice * p.Volume - f.Latest.AveragePrice * f.Latest.Volume
                    )
                    )
            }
            )
            .Select(x => new GetMarketForecastResponse(
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

        var response = new WrapperResponse<IQueryable<GetMarketForecastResponse>>(forecastsQuery);

        _logger.LogDebug("Successfully handled get market forecast query.");
        return await Task.FromResult(response);
    }
}
