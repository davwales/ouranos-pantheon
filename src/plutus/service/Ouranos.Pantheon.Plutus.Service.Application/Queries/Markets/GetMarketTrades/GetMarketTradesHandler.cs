using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketTrades;

public sealed class GetMarketTradesHandler
    : QueryHandler<GetMarketTradesInput, WrapperResponse<IQueryable<GetMarketTradesResponse>>>
{
    private readonly ILogger<GetMarketTradesHandler> _logger;
    private readonly IRepository<Market> _marketRepository;
    private readonly IRepository<Trade> _tradeRepository;

    public GetMarketTradesHandler(
        ILogger<GetMarketTradesHandler> logger,
        IRepository<Market> marketRepository,
        IRepository<Trade> tradeRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(marketRepository);
        Guard.Against.Null(tradeRepository);

        _logger = logger;
        _tradeRepository = tradeRepository;
        _marketRepository = marketRepository;
    }

    public override async Task<WrapperResponse<IQueryable<GetMarketTradesResponse>>> Handle(
        GetMarketTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle symbol statistics query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _marketRepository.Read(query.MarketId, cancellationToken);
        var flatTax = market.Taxes.Flat ?? new FlatTax(0, 0, 0);

        DateTimeOffset? since = query.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(query.Seconds.Value)
            : null;

        var tradeQuery = _tradeRepository.AsQueryable(cancellationToken)
            .Where(x => x.Symbol.MarketId == query.MarketId &&
                        (since == null || x.CreatedAt >= since)
            )
            .GroupBy(t => t.Symbol)
            .Select(g => new
                {
                    Symbol = g.Key,
                    TotalSpent = g.Sum(t => t.Price * t.Volume),
                    MinPrice = g.Min(t => t.Price),
                    MaxPrice = g.Max(t => t.Price),
                    TotalVolume = g.Sum(t => t.Volume),
                    NumTransactions = g.Count(),
                    Limit = g.Key.AdditionalFields.Limit ?? g.Sum(t => t.Volume)
                }
            )
            .Select(x => new
                {
                    x.Symbol,
                    x.TotalSpent,
                    x.MinPrice,
                    x.MaxPrice,
                    x.TotalVolume,
                    x.NumTransactions,
                    x.Limit,
                    Tax = x.MaxPrice * flatTax.Rate > flatTax.Maximum
                        ? flatTax.Maximum
                        : x.MaxPrice * flatTax.Rate > flatTax.Minimum
                            ? x.MaxPrice * flatTax.Rate
                            : 0
                }
            )
            .Select(x => new GetMarketTradesResponse(
                    x.Symbol,
                    x.TotalSpent,
                    x.MinPrice,
                    x.MaxPrice,
                    x.TotalVolume,
                    x.NumTransactions,
                    x.MaxPrice - x.MinPrice - x.Tax, // margin
                    x.TotalSpent / x.TotalVolume, // average price
                    (x.MaxPrice - x.MinPrice - x.Tax) / x.MinPrice, // roi
                    (x.MaxPrice - x.MinPrice - x.Tax) *
                    (x.TotalVolume > x.Limit ? x.Limit : x.TotalVolume), // total gain
                    x.Limit
                )
            );

        var response = new WrapperResponse<IQueryable<GetMarketTradesResponse>>(tradeQuery);

        _logger.LogDebug("Successfully handled symbol statistics request.");
        return await Task.FromResult(response);
    }
}