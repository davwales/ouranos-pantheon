using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using TickerQ.Utilities.Base;
using Snapshot = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades.MarketTradeSnapshot;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot;

public sealed class MarketTradeSnapshotJob
{
    private readonly ILogger<MarketTradeSnapshotJob> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly MarketTradeSnapshotOptions _options;

    public MarketTradeSnapshotJob(
        ILogger<MarketTradeSnapshotJob> logger,
        PlutusDbContext dbContext,
        IOptions<MarketTradeSnapshotOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContext = dbContext;
        _options = options.Value;
    }

    [TickerFunction("MarketTradeSnapshot_FifteenMinutes", "*/30 * * * * *")]
    public Task ExecuteFifteenMinutes(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.FifteenMinutes, ct);
    }

    [TickerFunction("MarketTradeSnapshot_OneHour", "0 */2 * * * *")]
    public Task ExecuteOneHour(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneHour, ct);
    }

    [TickerFunction("MarketTradeSnapshot_FourHours", "0 */5 * * * *")]
    public Task ExecuteFourHours(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.FourHours, ct);
    }

    [TickerFunction("MarketTradeSnapshot_OneDay", "0 */10 * * * *")]
    public Task ExecuteOneDay(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneDay, ct);
    }

    [TickerFunction("MarketTradeSnapshot_OneWeek", "0 */20 * * * *")]
    public Task ExecuteOneWeek(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneWeek, ct);
    }

    [TickerFunction("MarketTradeSnapshot_OneMonth", "0 */30 * * * *")]
    public Task ExecuteOneMonth(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneMonth, ct);
    }

    [TickerFunction("MarketTradeSnapshot_SixMonths", "0 */45 * * * *")]
    public Task ExecuteSixMonths(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.SixMonths, ct);
    }

    [TickerFunction("MarketTradeSnapshot_OneYear", "0 0 * * * *")]
    public Task ExecuteOneYear(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.OneYear, ct);
    }

    [TickerFunction("MarketTradeSnapshot_AllTime", "0 0 * * * *")]
    public Task ExecuteAllTime(TickerFunctionContext _, CancellationToken ct)
    {
        return RefreshAsync(TimeFrame.AllTime, ct);
    }

    private async Task RefreshAsync(TimeFrame frame, CancellationToken ct)
    {
        DateTimeOffset? since = frame.ToTimeSpan() is { } span
            ? DateTimeOffset.UtcNow - span
            : null;

        var taxRates = await _dbContext
            .Markets.AsNoTracking()
            .ToDictionaryAsync(m => m.Id, m => m.Taxes.Flat != null ? m.Taxes.Flat.Rate : 0m, ct);

        var aggregated = await AggregateBySymbolBatchAsync(since, ct);

        var snapshots = aggregated
            .Select(row =>
                Snapshot.Create(
                    row.Symbol.MarketId,
                    row.Symbol.Id,
                    frame,
                    row.TotalSpent,
                    row.MinPrice,
                    row.MaxPrice,
                    row.TotalVolume,
                    row.NumTx,
                    row.Limit,
                    row.MaxPrice * taxRates.GetValueOrDefault(row.Symbol.MarketId, 0m)
                )
            )
            .ToList();

        var existing = await _dbContext
            .MarketTradeSnapshots.Where(s => s.TimeFrame == frame)
            .ToListAsync(ct);
        _dbContext.MarketTradeSnapshots.RemoveRange(existing);
        _dbContext.MarketTradeSnapshots.AddRange(snapshots);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Refreshed {Count} snapshots for {Frame}.", snapshots.Count, frame);
    }

    private async Task<List<SymbolAggregate>> AggregateBySymbolBatchAsync(
        DateTimeOffset? since,
        CancellationToken ct
    )
    {
        var symbolIds = await _dbContext
            .Trades.AsNoTracking()
            .Where(t => since == null || t.Timestamp >= since)
            .Select(t => t.SymbolId)
            .Distinct()
            .ToListAsync(ct);

        var results = new List<SymbolAggregate>(symbolIds.Count);

        foreach (var batch in symbolIds.Chunk(_options.BatchSize))
        {
            var batchSet = batch.ToHashSet();

            var batchAggs = await _dbContext
                .Trades.AsNoTracking()
                .Where(t =>
                    batchSet.Contains(t.SymbolId) && (since == null || t.Timestamp >= since)
                )
                .Include(t => t.Symbol)
                .GroupBy(t => t.Symbol)
                .Select(g => new SymbolAggregate(
                    g.Key,
                    g.Sum(t => t.Price * t.Volume),
                    g.Min(t => t.Price),
                    g.Max(t => t.Price),
                    g.Sum(t => t.Volume),
                    g.Count(),
                    g.Key.AdditionalFields.Limit ?? g.Sum(t => t.Volume)
                ))
                .ToListAsync(ct);

            results.AddRange(batchAggs);
        }

        return results;
    }
}
