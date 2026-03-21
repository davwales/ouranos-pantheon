using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using TickerQ.Utilities.Base;
using Snapshot = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades.MarketTradeSnapshot;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot;

public sealed class MarketTradeSnapshotJob
{
    private readonly ILogger<MarketTradeSnapshotJob> _logger;
    private readonly PlutusDbContext _dbContext;

    public MarketTradeSnapshotJob(
        ILogger<MarketTradeSnapshotJob> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    [TickerFunction("MarketTradeSnapshot_FifteenMinutes", "*/30 * * * * *")]
    public Task ExecuteFifteenMinutes(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.FifteenMinutes, ct);

    [TickerFunction("MarketTradeSnapshot_OneHour", "0 */2 * * * *")]
    public Task ExecuteOneHour(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneHour, ct);

    [TickerFunction("MarketTradeSnapshot_FourHours", "0 */5 * * * *")]
    public Task ExecuteFourHours(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.FourHours, ct);

    [TickerFunction("MarketTradeSnapshot_OneDay", "0 */10 * * * *")]
    public Task ExecuteOneDay(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneDay, ct);

    [TickerFunction("MarketTradeSnapshot_OneWeek", "0 */20 * * * *")]
    public Task ExecuteOneWeek(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneWeek, ct);

    [TickerFunction("MarketTradeSnapshot_OneMonth", "0 */30 * * * *")]
    public Task ExecuteOneMonth(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneMonth, ct);

    [TickerFunction("MarketTradeSnapshot_SixMonths", "0 */45 * * * *")]
    public Task ExecuteSixMonths(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.SixMonths, ct);

    [TickerFunction("MarketTradeSnapshot_OneYear", "0 0 * * * *")]
    public Task ExecuteOneYear(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.OneYear, ct);

    [TickerFunction("MarketTradeSnapshot_AllTime", "0 0 * * * *")]
    public Task ExecuteAllTime(TickerFunctionContext _, CancellationToken ct)
        => RefreshAsync(TimeFrame.AllTime, ct);

    private async Task RefreshAsync(TimeFrame frame, CancellationToken ct)
    {
        DateTimeOffset? since = frame.ToTimeSpan() is { } span ? DateTimeOffset.UtcNow - span : null;

        var taxRates = await _dbContext.Markets.AsNoTracking()
            .ToDictionaryAsync(
                m => m.Id,
                m => m.Taxes.Flat != null ? m.Taxes.Flat.Rate : 0m,
                ct
            );

        var aggregated = await _dbContext.Trades.AsNoTracking()
            .Where(t => since == null || t.Timestamp >= since)
            .Include(t => t.Symbol)
            .GroupBy(t => t.Symbol)
            .Select(g => new
            {
                Symbol = g.Key,
                TotalSpent = g.Sum(t => t.Price * t.Volume),
                MinPrice = g.Min(t => t.Price),
                MaxPrice = g.Max(t => t.Price),
                TotalVolume = g.Sum(t => t.Volume),
                NumTx = g.Count(),
                Limit = g.Key.AdditionalFields.Limit ?? g.Sum(t => t.Volume)
            })
            .ToListAsync(ct);

        var snapshots = aggregated.Select(row => Snapshot.Create(
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
        )).ToList();

        await _dbContext.MarketTradeSnapshots.Where(s => s.TimeFrame == frame).ExecuteDeleteAsync(ct);
        _dbContext.MarketTradeSnapshots.AddRange(snapshots);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Refreshed {Count} snapshots for {Frame}.", snapshots.Count, frame);
    }
}
