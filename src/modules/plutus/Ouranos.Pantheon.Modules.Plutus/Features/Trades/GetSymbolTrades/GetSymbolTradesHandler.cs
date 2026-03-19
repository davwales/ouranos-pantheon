using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;

public sealed class GetSymbolTradesHandler : IPantheonHandler<GetSymbolTradesInput, GetSymbolTradesResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetSymbolTradesHandler> _logger;

    public GetSymbolTradesHandler(
        ILogger<GetSymbolTradesHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetSymbolTradesResponse> Handle(
        GetSymbolTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbol trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset? since = query.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(query.Seconds.Value)
            : null;

        var baseQuery = _dbContext.Trades
            .Where(t => t.SymbolId == query.SymbolId && (since == null || t.Timestamp >= since));

        var aggregatedStats = await baseQuery
            .GroupBy(t => 1)
            .Select(g => new
            {
                MinPrice = g.Min(t => t.Price),
                MaxPrice = g.Max(t => t.Price),
                TotalSpent = g.Sum(t => t.Price * t.Volume),
                Volume = g.Sum(t => t.Volume),
                NumTransactions = g.Count()
            }
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (aggregatedStats is null)
        {
            _logger.LogDebug("No trades found for symbol '{symbolId}'.", query.SymbolId);
            return new GetSymbolTradesResponse(0, 0, 0, 0, 0, 0, []);
        }

        var buckets = await GetBucketedTrades(baseQuery, query.NumBuckets, cancellationToken);

        var response = new GetSymbolTradesResponse(
            aggregatedStats.MinPrice,
            aggregatedStats.MaxPrice,
            aggregatedStats.Volume > 0
                ? aggregatedStats.TotalSpent / aggregatedStats.Volume
                : 0m,
            aggregatedStats.TotalSpent,
            aggregatedStats.Volume,
            aggregatedStats.NumTransactions,
            [
                .. buckets
                    .Select(b => new GetSymbolTradeBucketsResponse(
                            b.AveragePrice,
                            b.Volume,
                            b.TotalSpent,
                            b.MinPrice,
                            b.MaxPrice,
                            b.NumTransactions,
                            b.BucketStart
                        )
                    )
            ]
        );

        _logger.LogDebug("Successfully handled get symbol trades request.");
        return response;
    }

    private static async Task<List<BucketDto>> GetBucketedTrades(
        IQueryable<Trade> query,
        int numBuckets,
        CancellationToken cancellationToken
    )
    {
        var timeRange = await query
            .GroupBy(t => 1)
            .Select(g => new
            {
                StartTime = g.Min(t => t.Timestamp),
                EndTime = g.Max(t => t.Timestamp),
                Duration = g.Max(t => t.Timestamp) - g.Min(t => t.Timestamp)
            }
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (timeRange is null || timeRange.Duration <= TimeSpan.Zero)
        {
            return [];
        }

        var buckets = await query
            .GroupBy(t => TimescaleDbFunctions.TimeBucket(
                    CalculateSmartInterval(timeRange.Duration, numBuckets),
                    t.Timestamp
                )
            )
            .Select(group => new BucketDto(
                    group.First().SymbolId,
                    group.Key,
                    group.Sum(x => x.Price * x.Volume),
                    group.Sum(x => x.Volume),
                    group.Min(x => x.Price),
                    group.Max(x => x.Price),
                    group.Count(),
                    group.Sum(x => x.Price * x.Volume) / group.Sum(x => x.Volume),
                    group.Max(x => x.Price) - group.Min(x => x.Price)
                )
            )
            .ToListAsync(cancellationToken);

        return [.. buckets.OrderBy(b => b.BucketStart)];
    }

    private static TimeSpan CalculateSmartInterval(TimeSpan duration, int numBuckets)
    {
        if (duration.TotalSeconds <= 0)
        {
            return TimeSpan.FromMinutes(5);
        }

        var targetBucketSize = duration.TotalSeconds / numBuckets;

        return targetBucketSize switch
        {
            <= 60 => TimeSpan.FromSeconds(Math.Max(1, Math.Floor(targetBucketSize / 60) * 60)),
            <= 3600 => TimeSpan.FromMinutes(Math.Max(1, Math.Floor(targetBucketSize / 60))),
            <= 86400 => TimeSpan.FromMinutes(Math.Max(5, Math.Floor(targetBucketSize / 3600) * 60)),
            <= 604800 => TimeSpan.FromHours(Math.Max(1, Math.Floor(targetBucketSize / 3600))),
            <= 2592000 => TimeSpan.FromHours(Math.Max(6, Math.Floor(targetBucketSize / 86400) * 24)),
            <= 31536000 => TimeSpan.FromDays(Math.Max(1, Math.Floor(targetBucketSize / 86400))),
            _ => TimeSpan.FromDays(Math.Max(7, Math.Floor(targetBucketSize / 604800) * 7))
        };
    }
}
