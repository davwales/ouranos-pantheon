using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
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

        DateTimeOffset? since = query.TimeFrame.ToTimeSpan() is { } span
            ? DateTimeOffset.UtcNow - span
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
                            b.BucketStart,
                            b.OpenPrice,
                            b.ClosePrice
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

        var interval = SmartIntervalCalculator.Calculate(timeRange.Duration, numBuckets);

        var buckets = await query
            .GroupBy(t => TimescaleDbFunctions.TimeBucket(interval, t.Timestamp))
            .Select(group => new BucketDto(
                    group.First().SymbolId,
                    group.Key,
                    group.Sum(x => x.Price * x.Volume),
                    group.Sum(x => x.Volume),
                    group.Min(x => x.Price),
                    group.Max(x => x.Price),
                    group.Count(),
                    group.Sum(x => x.Price * x.Volume) / group.Sum(x => x.Volume),
                    group.Max(x => x.Price) - group.Min(x => x.Price),
                    group.OrderBy(x => x.Timestamp).Select(x => x.Price).First(),
                    group.OrderByDescending(x => x.Timestamp).Select(x => x.Price).First()
                )
            )
            .ToListAsync(cancellationToken);

        return [.. buckets.OrderBy(b => b.BucketStart)];
    }
}
