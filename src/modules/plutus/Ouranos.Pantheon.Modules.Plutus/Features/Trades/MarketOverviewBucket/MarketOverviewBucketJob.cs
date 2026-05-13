using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using TickerQ.Utilities.Base;
using Bucket = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades.MarketOverviewBucket;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket;

public sealed class MarketOverviewBucketJob
{
    private readonly ILogger<MarketOverviewBucketJob> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<MarketOverviewBucketOptions> _options;

    public MarketOverviewBucketJob(
        ILogger<MarketOverviewBucketJob> logger,
        PlutusDbContext dbContext,
        IOptions<MarketOverviewBucketOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContext = dbContext;
        _options = options;
    }

    [TickerFunction("MarketOverviewBucket_FifteenMinutes", "*/30 * * * * *")]
    public Task ExecuteFifteenMinutes(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.FifteenMinutes, ct);
    }

    [TickerFunction("MarketOverviewBucket_OneHour", "0 */2 * * * *")]
    public Task ExecuteOneHour(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneHour, ct);
    }

    [TickerFunction("MarketOverviewBucket_FourHours", "0 */5 * * * *")]
    public Task ExecuteFourHours(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.FourHours, ct);
    }

    [TickerFunction("MarketOverviewBucket_OneDay", "0 */10 * * * *")]
    public Task ExecuteOneDay(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneDay, ct);
    }

    [TickerFunction("MarketOverviewBucket_OneWeek", "0 */20 * * * *")]
    public Task ExecuteOneWeek(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneWeek, ct);
    }

    [TickerFunction("MarketOverviewBucket_OneMonth", "0 */30 * * * *")]
    public Task ExecuteOneMonth(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneMonth, ct);
    }

    [TickerFunction("MarketOverviewBucket_SixMonths", "0 */45 * * * *")]
    public Task ExecuteSixMonths(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.SixMonths, ct);
    }

    [TickerFunction("MarketOverviewBucket_OneYear", "0 0 * * * *")]
    public Task ExecuteOneYear(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneYear, ct);
    }

    [TickerFunction("MarketOverviewBucket_AllTime", "0 0 * * * *")]
    public Task ExecuteAllTime(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.AllTime, ct);
    }

    private async Task RefreshAsync(TimeFrame frame, CancellationToken ct)
    {
        DateTimeOffset? since = frame.ToTimeSpan() is { } span
            ? DateTimeOffset.UtcNow - span
            : null;

        var marketIds = await _dbContext.Markets.AsNoTracking().Select(m => m.Id).ToListAsync(ct);

        var totalBuckets = 0;

        foreach (var marketId in marketIds)
        {
            var buckets = await ComputeBucketsForMarketAsync(marketId, since, frame, ct);

            var existing = await _dbContext
                .MarketOverviewBuckets.Where(b => b.TimeFrame == frame && b.MarketId == marketId)
                .ToListAsync(ct);

            _dbContext.MarketOverviewBuckets.RemoveRange(existing);
            _dbContext.MarketOverviewBuckets.AddRange(buckets);
            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();

            totalBuckets += buckets.Count;
        }

        _logger.LogInformation(
            "Refreshed {Count} overview buckets across {Markets} market(s) for {Frame}.",
            totalBuckets,
            marketIds.Count,
            frame
        );
    }

    private async Task<List<Bucket>> ComputeBucketsForMarketAsync(
        Id<Market> marketId,
        DateTimeOffset? since,
        TimeFrame frame,
        CancellationToken ct
    )
    {
        var baseQuery = _dbContext
            .Trades.AsNoTracking()
            .Where(t => t.Symbol.MarketId == marketId && (since == null || t.Timestamp >= since));

        var timeRange = await baseQuery
            .GroupBy(t => 1)
            .Select(g => new { Min = g.Min(t => t.Timestamp), Max = g.Max(t => t.Timestamp) })
            .FirstOrDefaultAsync(ct);

        if (timeRange is null || timeRange.Max <= timeRange.Min)
        {
            return [];
        }

        var duration = timeRange.Max - timeRange.Min;
        var interval = SmartIntervalCalculator.Calculate(duration, _options.Value.NumBuckets);
        var chunkThreshold = TimeSpan.FromDays(_options.Value.ChunkThresholdDays);

        var aggregates =
            duration > chunkThreshold
                ? await AggregateInChunksAsync(marketId, timeRange.Min, timeRange.Max, interval, ct)
                : await AggregateQuery(baseQuery, interval).ToListAsync(ct);

        return
        [
            .. aggregates
                .OrderBy(a => a.BucketStart)
                .Select(a =>
                    Bucket.Create(
                        marketId,
                        frame,
                        a.BucketStart,
                        a.AveragePrice,
                        a.Volume,
                        a.TotalSpent,
                        a.NumTransactions
                    )
                ),
        ];
    }

    private async Task<List<MarketBucketAggregate>> AggregateInChunksAsync(
        Id<Market> marketId,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        TimeSpan interval,
        CancellationToken ct
    )
    {
        var chunkSize = TimeSpan.FromDays(_options.Value.ChunkDays);
        var accumulated = new Dictionary<DateTimeOffset, MarketBucketAggregate>();

        var chunkStart = rangeStart;
        while (chunkStart < rangeEnd)
        {
            var chunkEnd = chunkStart + chunkSize;
            var start = chunkStart;

            var chunkQuery = _dbContext
                .Trades.AsNoTracking()
                .Where(t =>
                    t.Symbol.MarketId == marketId && t.Timestamp >= start && t.Timestamp < chunkEnd
                );

            var chunkAggregates = await AggregateQuery(chunkQuery, interval).ToListAsync(ct);

            foreach (var agg in chunkAggregates)
            {
                if (!accumulated.TryGetValue(agg.BucketStart, out var existing))
                {
                    accumulated[agg.BucketStart] = agg;
                    continue;
                }

                var mergedTotalSpent = existing.TotalSpent + agg.TotalSpent;
                var mergedVolume = existing.Volume + agg.Volume;

                accumulated[agg.BucketStart] = new MarketBucketAggregate(
                    agg.BucketStart,
                    mergedTotalSpent,
                    mergedVolume,
                    existing.NumTransactions + agg.NumTransactions,
                    mergedVolume > 0 ? mergedTotalSpent / mergedVolume : 0
                );
            }

            chunkStart = chunkEnd;
        }

        return [.. accumulated.Values];
    }

    private static IQueryable<MarketBucketAggregate> AggregateQuery(
        IQueryable<Trade> query,
        TimeSpan interval
    )
    {
        return query
            .GroupBy(t => TimescaleDbFunctions.TimeBucket(interval, t.Timestamp))
            .Select(g => new MarketBucketAggregate(
                g.Key,
                g.Sum(t => t.Price * t.Volume),
                g.Sum(t => t.Volume),
                g.Count(),
                g.Sum(t => t.Price * t.Volume) / g.Sum(t => t.Volume)
            ));
    }
}
