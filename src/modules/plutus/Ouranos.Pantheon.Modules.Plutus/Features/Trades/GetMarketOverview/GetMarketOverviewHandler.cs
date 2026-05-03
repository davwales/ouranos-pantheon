using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview;

public sealed class GetMarketOverviewHandler : IPantheonHandler<GetMarketOverviewInput, GetMarketOverviewResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetMarketOverviewHandler> _logger;

    public GetMarketOverviewHandler(
        ILogger<GetMarketOverviewHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetMarketOverviewResponse> Handle(
        GetMarketOverviewInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get market overview query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var buckets = await _dbContext.MarketOverviewBuckets
            .AsNoTracking()
            .Where(b => b.MarketId == query.MarketId && b.TimeFrame == query.TimeFrame)
            .OrderBy(b => b.BucketStart)
            .ToListAsync(cancellationToken);

        if (buckets.Count == 0)
        {
            _logger.LogDebug(
                "No overview buckets found for market '{MarketId}' / {TimeFrame}.",
                query.MarketId,
                query.TimeFrame
            );
            return new GetMarketOverviewResponse(0, 0, 0, 0, []);
        }

        var totalSpent = buckets.Sum(b => b.TotalSpent);
        var volume = buckets.Sum(b => b.Volume);
        var numTransactions = buckets.Sum(b => b.NumTransactions);
        var averagePrice = volume > 0 ? totalSpent / volume : 0m;

        var trades = buckets
            .Select(b => new GetMarketOverviewBucketResponse(
                    b.AveragePrice,
                    b.Volume,
                    b.TotalSpent,
                    b.NumTransactions,
                    b.BucketStart
                )
            )
            .ToList();

        _logger.LogDebug("Successfully handled get market overview request.");
        return new GetMarketOverviewResponse(averagePrice, totalSpent, volume, numTransactions, trades);
    }
}
