using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using TickerQ.Utilities.Base;
using Bucket = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades.MarketOverviewBucket;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket;

public sealed class MarketOverviewBucketJob
{
    private readonly ILogger<MarketOverviewBucketJob> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly MarketOverviewBucketOptions _options;

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
        _options = options.Value;
    }

    [TickerFunction("MarketOverviewBucket_FifteenMinutes", "*/30 * * * * *")]
    public Task ExecuteFifteenMinutes(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.FifteenMinutes, ct);

    [TickerFunction("MarketOverviewBucket_OneHour", "0 */2 * * * *")]
    public Task ExecuteOneHour(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneHour, ct);

    [TickerFunction("MarketOverviewBucket_FourHours", "0 */5 * * * *")]
    public Task ExecuteFourHours(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.FourHours, ct);

    [TickerFunction("MarketOverviewBucket_OneDay", "0 */10 * * * *")]
    public Task ExecuteOneDay(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneDay, ct);

    [TickerFunction("MarketOverviewBucket_OneWeek", "0 */20 * * * *")]
    public Task ExecuteOneWeek(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneWeek, ct);

    [TickerFunction("MarketOverviewBucket_OneMonth", "0 */30 * * * *")]
    public Task ExecuteOneMonth(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneMonth, ct);

    [TickerFunction("MarketOverviewBucket_SixMonths", "0 */45 * * * *")]
    public Task ExecuteSixMonths(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.SixMonths, ct);

    [TickerFunction("MarketOverviewBucket_OneYear", "0 0 * * * *")]
    public Task ExecuteOneYear(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneYear, ct);

    [TickerFunction("MarketOverviewBucket_AllTime", "0 0 * * * *")]
    public Task ExecuteAllTime(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.AllTime, ct);

    private async Task RefreshAsync(TimeFrame frame, CancellationToken ct)
    {
        DateTimeOffset? since = frame.ToTimeSpan() is { } span ? DateTimeOffset.UtcNow - span : null;

        var marketIds = await _dbContext.Markets.AsNoTracking()
            .Select(m => m.Id)
            .ToListAsync(ct);

        var newBuckets = new List<Bucket>();

        foreach (var marketId in marketIds)
        {
            var buckets = await ComputeBucketsForMarketAsync(marketId, since, frame, ct);
            newBuckets.AddRange(buckets);
        }

        var existing = await _dbContext.MarketOverviewBuckets
            .Where(b => b.TimeFrame == frame)
            .ToListAsync(ct);

        _dbContext.MarketOverviewBuckets.RemoveRange(existing);
        _dbContext.MarketOverviewBuckets.AddRange(newBuckets);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Refreshed {Count} overview buckets across {Markets} market(s) for {Frame}.",
            newBuckets.Count,
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
        var query = _dbContext.Trades.AsNoTracking()
            .Where(t => t.Symbol.MarketId == marketId && (since == null || t.Timestamp >= since));

        var timeRange = await query
            .GroupBy(t => 1)
            .Select(g => new
            {
                Duration = g.Max(t => t.Timestamp) - g.Min(t => t.Timestamp)
            })
            .FirstOrDefaultAsync(ct);

        if (timeRange is null || timeRange.Duration <= TimeSpan.Zero)
        {
            return [];
        }

        var interval = SmartIntervalCalculator.Calculate(timeRange.Duration, _options.NumBuckets);

        var aggregates = await query
            .GroupBy(t => TimescaleDbFunctions.TimeBucket(interval, t.Timestamp))
            .Select(g => new MarketBucketAggregate(
                g.Key,
                g.Sum(t => t.Price * t.Volume),
                g.Sum(t => t.Volume),
                g.Min(t => t.Price),
                g.Max(t => t.Price),
                g.Count(),
                g.Sum(t => t.Price * t.Volume) / g.Sum(t => t.Volume)
            ))
            .ToListAsync(ct);

        return [.. aggregates
            .OrderBy(a => a.BucketStart)
            .Select(a => Bucket.Create(
                marketId,
                frame,
                a.BucketStart,
                a.AveragePrice,
                a.MinPrice,
                a.MaxPrice,
                a.Volume,
                a.TotalSpent,
                a.NumTransactions
            ))];
    }
}
