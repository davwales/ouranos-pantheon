using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

/// <summary>
///     Loads backtest data using DB-side TimescaleDB aggregations
///     instead of materializing raw trades into memory.
///     Returns pre-indexed data structures for O(1) lookups in the hot loop.
/// </summary>
public sealed class BacktestDataQueryService : IBacktestDataQueryService
{
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
        CancellationToken cancellationToken,
        int lookbackDays = 0
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var market = await dbContext
            .Markets.AsNoTracking()
            .FirstAsync(m => m.Id == marketId, cancellationToken);

        var symbols = await dbContext
            .Symbols.AsNoTracking()
            .Where(s => s.MarketId == marketId)
            .ToListAsync(cancellationToken);

        var symbolIds = symbols.Select(s => s.Id).ToList();

        var effectiveStart = lookbackDays > 0 ? startDate.AddDays(-lookbackDays) : startDate;

        var snapshotsTask = LoadSnapshotsAsync(symbolIds, marketId, cancellationToken);
        var forecastsTask = LoadForecastsAsync(symbolIds, marketId, cancellationToken);
        var dailyPricesTask = LoadDailyPricesAsync(
            symbolIds,
            effectiveStart,
            endDate,
            cancellationToken
        );
        var dailyAggregatesTask = LoadDailyAggregatesAsync(
            symbolIds,
            effectiveStart,
            endDate,
            cancellationToken
        );

        await Task.WhenAll(snapshotsTask, forecastsTask, dailyPricesTask, dailyAggregatesTask);

        var snapshots = await snapshotsTask;
        var forecasts = await forecastsTask;
        var dailyPrices = await dailyPricesTask;
        var dailyAggregates = await dailyAggregatesTask;

        _logger.LogDebug(
            "Loaded backtest data: {symbolCount} symbols, {snapshotCount} snapshots, "
                + "{forecastCount} forecasts, {dailyPriceCount} daily prices, "
                + "{dailyAggregateCount} daily aggregates.",
            symbols.Count,
            snapshots.Count,
            forecasts.Count,
            dailyPrices.Count,
            dailyAggregates.Count
        );

        return BacktestData.FromRaw(
            market,
            symbols,
            snapshots,
            forecasts,
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

        return await dbContext
            .MarketTradeSnapshots.AsNoTracking()
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

        return await dbContext
            .Forecasts.AsNoTracking()
            .Include(f => f.Predictions)
            .Where(f => symbolIds.Contains(f.SymbolId) && f.MarketId == marketId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Loads one closing price per symbol per day using TimescaleDB
    ///     <c>time_bucket</c> and <c>last()</c> aggregate.
    ///     Fully server-side: no client-side grouping or sorting.
    /// </summary>
    private async Task<List<DailyPrice>> LoadDailyPricesAsync(
        List<Id<Symbol>> symbolIds,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var command = RawSqlCommand
            .FromSql(
                """
                SELECT symbol_id,
                       time_bucket('1 day', "timestamp") AS date,
                       last(price, "timestamp") AS close_price
                FROM plutus.trades
                WHERE symbol_id = ANY(@symbolIds)
                  AND "timestamp" >= @startDate
                  AND "timestamp" <= @endDate
                GROUP BY symbol_id, time_bucket('1 day', "timestamp")
                """
            )
            .WithIds("@symbolIds", symbolIds)
            .WithDateTimeOffset("@startDate", startDate)
            .WithDateTimeOffset("@endDate", endDate);

        var rows = await dbContext.Database.ExecuteQueryAsync<DailyPriceRow>(
            command,
            cancellationToken
        );

        return
        [
            .. rows.Select(d => new DailyPrice(
                new Id<Symbol>(d.SymbolId.ToString()),
                DateOnly.FromDateTime(d.Date.UtcDateTime),
                d.ClosePrice
            )),
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

        var command = RawSqlCommand
            .FromSql(
                """
                SELECT symbol_id,
                       time_bucket('1 day', "timestamp") AS date,
                       AVG(price) AS average_price,
                       MIN(price) AS min_price,
                       MAX(price) AS max_price,
                       SUM(volume) AS total_volume
                FROM plutus.trades
                WHERE symbol_id = ANY(@symbolIds)
                  AND "timestamp" >= @startDate
                  AND "timestamp" <= @endDate
                GROUP BY symbol_id, time_bucket('1 day', "timestamp")
                """
            )
            .WithIds("@symbolIds", symbolIds)
            .WithDateTimeOffset("@startDate", startDate)
            .WithDateTimeOffset("@endDate", endDate);

        var rows = await dbContext.Database.ExecuteQueryAsync<DailyTradeAggregateRow>(
            command,
            cancellationToken
        );

        return
        [
            .. rows.Select(d => new DailyTradeAggregate(
                new Id<Symbol>(d.SymbolId.ToString()),
                DateOnly.FromDateTime(d.Date.UtcDateTime),
                d.AveragePrice,
                d.MinPrice,
                d.MaxPrice,
                d.TotalVolume
            )),
        ];
    }
}
