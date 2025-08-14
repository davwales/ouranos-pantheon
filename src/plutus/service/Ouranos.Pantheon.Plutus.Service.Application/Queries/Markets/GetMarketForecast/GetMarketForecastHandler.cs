using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketForecast;

public sealed class GetMarketForecastHandler
    : QueryHandler<GetMarketForecastInput, WrapperResponse<IQueryable<GetMarketForecastResponse>>>
{
    private readonly IRepository<Forecast> _forecastRepository;
    private readonly ILogger<GetMarketForecastHandler> _logger;
    private readonly IRepository<Market> _marketRepository;

    public GetMarketForecastHandler(
        ILogger<GetMarketForecastHandler> logger,
        IRepository<Forecast> forecastRepository,
        IRepository<Market> marketRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(forecastRepository);
        Guard.Against.Null(marketRepository);

        _logger = logger;
        _forecastRepository = forecastRepository;
        _marketRepository = marketRepository;
    }

    public override async Task<WrapperResponse<IQueryable<GetMarketForecastResponse>>> Handle(
        GetMarketForecastInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get market forecast query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _marketRepository.Read(query.MarketId, cancellationToken);
        var flatTax = market.Taxes.Flat ?? new FlatTax(0, 0, 0);

        var forecastsQuery = _forecastRepository
            .AsQueryable(cancellationToken)
            .Where(f => f.MarketId == query.MarketId && f.Predictions.Count >= 7)
            .Select(f => new
                {
                    f.Id,
                    f.MarketId,
                    f.SymbolId,
                    f.SymbolName,
                    f.SymbolSubcode,
                    f.Latest,
                    Predictions = f.Predictions.Select(p => new
                        {
                            p.AveragePrice,
                            p.MaxPrice,
                            p.MinPrice,
                            p.Volume,
                            Margin = p.AveragePrice - (
                                p.AveragePrice * flatTax.Rate > flatTax.Maximum
                                    ? flatTax.Maximum
                                    : p.AveragePrice * flatTax.Rate > flatTax.Minimum
                                        ? p.AveragePrice * flatTax.Rate
                                        : 0
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