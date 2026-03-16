using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;

public sealed class GetMarketTradesHandler
    : QueryHandler<GetMarketTradesInput, WrapperResponse<IQueryable<GetMarketTradesResponse>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetMarketTradesHandler> _logger;

    public GetMarketTradesHandler(
        ILogger<GetMarketTradesHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<GetMarketTradesResponse>>> Handle(
        GetMarketTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle symbol statistics query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _dbContext.Markets.FirstOrDefaultAsync(m => m.Id == query.MarketId, cancellationToken);
        Guard.Against.NotFound(query.MarketId, market);

        var flatTax = market.Taxes.Flat ?? new FlatTax(0, 0, 0);

        DateTimeOffset? since = query.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(query.Seconds.Value)
            : null;

        var tradeQuery = _dbContext.Trades
            .Where(x => x.Symbol.MarketId == query.MarketId &&
                        (since == null || x.Timestamp >= since)
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
            .AsEnumerable()
            .Select(x => new GetMarketTradesResponse(
                    x.Symbol,
                    x.TotalSpent,
                    x.MinPrice,
                    x.MaxPrice,
                    x.TotalVolume,
                    x.NumTransactions,
                    x.Limit,
                    x.MaxPrice * flatTax.Rate
                )
            );

        var response = new WrapperResponse<IQueryable<GetMarketTradesResponse>>(tradeQuery.AsQueryable());

        _logger.LogDebug("Successfully handled symbol statistics request.");
        return await Task.FromResult(response);
    }
}
