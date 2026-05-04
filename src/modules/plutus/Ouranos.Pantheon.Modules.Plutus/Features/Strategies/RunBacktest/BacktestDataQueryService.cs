using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

/// <summary>
///     Loads backtest data using DB-side TimescaleDB aggregations
///     instead of materializing raw trades into memory.
///     Returns pre-indexed data structures for O(1) lookups in the hot loop.
/// </summary>
public sealed class BacktestDataQueryService
{
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly ILogger<BacktestDataQueryService> _logger;

    public BacktestDataQueryService(
        ILogger<BacktestDataQueryService> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<BacktestData> LoadDataAsync(
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var market = await dbContext.Markets.AsNoTracking()
            .FirstAsync(m => m.Id == marketId, cancellationToken);

        var symbols = await dbContext.Symbols
            .AsNoTracking()
            .Where(s => s.MarketId == marketId)
            .ToListAsync(cancellationToken);

        var symbolIds = symbols.Select(s => s.Id).ToList();

        var snapshotsTask = LoadSnapshotsAsync(symbolIds, marketId, cancellationToken);
        var forecastsTask = LoadForecastsAsync(symbolIds, marketId, cancellationToken);
        var signalsTask = LoadSignalsAsync(symbolIds, cancellationToken);
        var dailyPricesTask = LoadDailyPricesAsync(symbolIds, startDate, endDate, cancellationToken);
        var dailyAggregatesTask = LoadDailyAggregatesAsync(symbolIds, startDate, endDate, cancellationToken);

        await Task.WhenAll(snapshotsTask, forecastsTask, signalsTask, dailyPricesTask, dailyAggregatesTask);

        var snapshots = await snapshotsTask;
        var forecasts = await forecastsTask;
        var signals = await signalsTask;
        var dailyPrices = await dailyPricesTask;
        var dailyAggregates = await dailyAggregatesTask;

        _logger.LogDebug(
            "Loaded backtest data: {symbolCount} symbols, {snapshotCount} snapshots, " +
            "{forecastCount} forecasts, {signalCount} signals, {dailyPriceCount} daily prices, " +
            "{dailyAggregateCount} daily aggregates.",
            symbols.Count,
            snapshots.Count,
            forecasts.Count,
            signals.Count,
            dailyPrices.Count,
            dailyAggregates.Count
        );

        return BacktestData.FromRaw(
            market,
            symbols,
            snapshots,
            forecasts,
            signals,
            dailyPrices,
            dailyAggregates
        );
    }

    private async Task<List<MarketTradeSnapshot>> LoadSnapshotsAsync(
        List<Id<Symbol>> symbolIds,
        Id<Market> marketId,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.MarketTradeSnapshots
            .AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId) && s.MarketId == marketId)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<Forecast>> LoadForecastsAsync(
        List<Id<Symbol>> symbolIds,
        Id<Market> marketId,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Forecasts
            .AsNoTracking()
            .Include(f => f.Predictions)
            .Where(f => symbolIds.Contains(f.SymbolId) && f.MarketId == marketId)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<Signal>> LoadSignalsAsync(
        List<Id<Symbol>> symbolIds,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Signals
            .AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Loads one closing price per symbol per day using TimescaleDB time_bucket.
    ///     Uses the last trade of each day as the closing price.
    ///     Replaces loading millions of raw Trade rows and doing MaxBy in memory.
    /// </summary>
    private async Task<List<DailyPrice>> LoadDailyPricesAsync(
        List<Id<Symbol>> symbolIds,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var grouped = await dbContext.Trades
            .AsNoTracking()
            .Where(t => symbolIds.Contains(t.SymbolId)
                        && t.Timestamp >= startDate
                        && t.Timestamp <= endDate
            )
            .GroupBy(t => new { t.SymbolId, Day = TimescaleDbFunctions.TimeBucket(OneDay, t.Timestamp) })
            .Select(g => new DailyPriceDto(
                    g.Key.SymbolId,
                    g.Key.Day,
                    g.OrderByDescending(t => t.Timestamp).First().Price
                )
            )
            .ToListAsync(cancellationToken);

        return
        [
            .. grouped.Select(d => new DailyPrice(
                    d.SymbolId,
                    DateOnly.FromDateTime(d.Date.UtcDateTime),
                    d.ClosePrice
                )
            )
        ];
    }

    /// <summary>
    ///     Loads daily aggregated trade data (OHLCV) per symbol using TimescaleDB time_bucket.
    ///     Used to build PriceBuckets for the scoring window without loading raw trades.
    /// </summary>
    private async Task<List<DailyTradeAggregate>> LoadDailyAggregatesAsync(
        List<Id<Symbol>> symbolIds,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var grouped = await dbContext.Trades
            .AsNoTracking()
            .Where(t => symbolIds.Contains(t.SymbolId)
                        && t.Timestamp >= startDate
                        && t.Timestamp <= endDate
            )
            .GroupBy(t => new { t.SymbolId, Day = TimescaleDbFunctions.TimeBucket(OneDay, t.Timestamp) })
            .Select(g => new DailyTradeAggregateDto(
                    g.Key.SymbolId,
                    g.Key.Day,
                    g.Average(t => t.Price),
                    g.Min(t => t.Price),
                    g.Max(t => t.Price),
                    g.Sum(t => t.Volume)
                )
            )
            .ToListAsync(cancellationToken);

        return
        [
            .. grouped
                .Select(d => new DailyTradeAggregate(
                        d.SymbolId,
                        DateOnly.FromDateTime(d.Date.UtcDateTime),
                        d.AveragePrice,
                        d.MinPrice,
                        d.MaxPrice,
                        d.TotalVolume
                    )
                )
        ];
    }
}