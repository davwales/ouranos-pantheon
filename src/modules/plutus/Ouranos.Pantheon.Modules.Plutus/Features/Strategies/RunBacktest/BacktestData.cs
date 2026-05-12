using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

/// <summary>
///     Pre-indexed backtest data optimized for O(1) lookups in the hot loop.
///     Replaces flat List lookups with Dictionary lookups by SymbolId,
///     and replaces raw Trade materialization with pre-aggregated daily prices and trade aggregates.
/// </summary>
public sealed class BacktestData
{
    public Market Market { get; }
    public List<Symbol> Symbols { get; }
    public List<MarketTradeSnapshot> Snapshots { get; }
    public List<Forecast> Forecasts { get; }

    /// <summary>
    ///     Daily closing prices indexed by (SymbolId, Date).
    ///     Available for diagnostics; price resolution in the engine
    ///     uses GetLatestPrice (average price) to avoid outlier trades.
    /// </summary>
    public Dictionary<(Id<Symbol> SymbolId, DateOnly Date), decimal> DailyPricesByDate { get; }

    /// <summary>
    ///     Daily trade aggregates indexed by SymbolId.
    ///     Each symbol's aggregates are sorted by date for efficient window slicing.
    ///     Used to build PriceBuckets without loading raw trades.
    /// </summary>
    public Dictionary<Id<Symbol>, List<DailyTradeAggregate>> AggregatesBySymbol { get; }

    /// <summary>
    ///     Snapshots indexed by SymbolId for O(1) lookup.
    /// </summary>
    public Dictionary<Id<Symbol>, List<MarketTradeSnapshot>> SnapshotsBySymbol { get; }

    /// <summary>
    ///     Forecasts indexed by SymbolId for O(1) lookup.
    /// </summary>
    public Dictionary<Id<Symbol>, Forecast> ForecastBySymbol { get; }

    private BacktestData(
        Market market,
        List<Symbol> symbols,
        Dictionary<Id<Symbol>, List<MarketTradeSnapshot>> snapshotsBySymbol,
        Dictionary<Id<Symbol>, Forecast> forecastBySymbol,
        Dictionary<(Id<Symbol> SymbolId, DateOnly Date), decimal> dailyPricesByDate,
        Dictionary<Id<Symbol>, List<DailyTradeAggregate>> aggregatesBySymbol
    )
    {
        Market = market;
        Symbols = symbols;
        SnapshotsBySymbol = snapshotsBySymbol;
        ForecastBySymbol = forecastBySymbol;
        DailyPricesByDate = dailyPricesByDate;
        AggregatesBySymbol = aggregatesBySymbol;

        // Keep flat lists for backward compatibility with methods that iterate all items
        Snapshots = snapshotsBySymbol.SelectMany(kvp => kvp.Value).ToList();
        Forecasts = forecastBySymbol.Values.ToList();
    }

    /// <summary>
    ///     Creates <see cref="BacktestData" /> from raw query results,
    ///     indexing all collections by SymbolId for O(1) lookups.
    /// </summary>
    public static BacktestData FromRaw(
        Market market,
        List<Symbol> symbols,
        List<MarketTradeSnapshot> snapshots,
        List<Forecast> forecasts,
        List<DailyPrice> dailyPrices,
        List<DailyTradeAggregate> dailyAggregates
    )
    {
        var snapshotsBySymbol = snapshots
            .GroupBy(s => s.SymbolId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var forecastBySymbol = forecasts
            .GroupBy(f => f.SymbolId)
            .ToDictionary(g => g.Key, g => g.First());

        var dailyPricesByDate = dailyPrices.ToDictionary(
            dp => (dp.SymbolId, dp.Date),
            dp => dp.ClosePrice
        );

        var aggregatesBySymbol = dailyAggregates
            .GroupBy(a => a.SymbolId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.Date).ToList());

        return new BacktestData(
            market,
            symbols,
            snapshotsBySymbol,
            forecastBySymbol,
            dailyPricesByDate,
            aggregatesBySymbol
        );
    }

    /// <summary>
    ///     Gets the most recent average price for a symbol before or on a given date.
    ///     Uses binary search on the sorted daily aggregate list.
    ///     Preferred over raw close price to avoid outlier trade prices
    ///     skewing backtest results.
    /// </summary>
    public decimal GetLatestPrice(Id<Symbol> symbolId, DateTimeOffset date)
    {
        if (!AggregatesBySymbol.TryGetValue(symbolId, out var aggregates) || aggregates.Count == 0)
        {
            return 0m;
        }

        var dateOnly = DateOnly.FromDateTime(date.UtcDateTime);

        // Binary search for the last aggregate on or before the given date
        var idx = aggregates.FindLastIndex(a => a.Date <= dateOnly);
        if (idx < 0)
        {
            return 0m;
        }

        return aggregates[idx].AveragePrice;
    }

    /// <summary>
    ///     Gets daily trade aggregates for a symbol within a date window.
    ///     Uses the pre-indexed aggregates by symbol for efficient range lookup.
    /// </summary>
    public List<DailyTradeAggregate> GetWindowAggregates(
        Id<Symbol> symbolId,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd
    )
    {
        if (!AggregatesBySymbol.TryGetValue(symbolId, out var aggregates))
        {
            return [];
        }

        var startDate = DateOnly.FromDateTime(windowStart.UtcDateTime);
        var endDate = DateOnly.FromDateTime(windowEnd.UtcDateTime);

        return aggregates.Where(a => a.Date >= startDate && a.Date <= endDate).ToList();
    }

    /// <summary>
    ///     Gets snapshots for a symbol that were available on or before the given date,
    ///     preventing lookahead bias. Uses each snapshot's CreatedAt to filter.
    /// </summary>
    public (
        MarketTradeSnapshot? Short,
        MarketTradeSnapshot? Medium,
        MarketTradeSnapshot? Long
    ) GetSnapshotsForSymbol(Id<Symbol> symbolId, DateTimeOffset asOfDate)
    {
        if (!SnapshotsBySymbol.TryGetValue(symbolId, out var symbolSnaps))
        {
            return (null, null, null);
        }

        var availableSnaps = symbolSnaps.Where(s => s.CreatedAt <= asOfDate).ToList();

        return (
            availableSnaps.LastOrDefault(s => s.TimeFrame == TimeFrame.OneHour),
            availableSnaps.LastOrDefault(s => s.TimeFrame == TimeFrame.OneWeek),
            availableSnaps.LastOrDefault(s => s.TimeFrame == TimeFrame.OneMonth)
        );
    }

    /// <summary>
    ///     Gets the forecast for a symbol that was available on or before the given date,
    ///     preventing lookahead bias. Uses the forecast's CreatedAt to filter.
    /// </summary>
    public Forecast? GetForecastForSymbol(Id<Symbol> symbolId, DateTimeOffset asOfDate)
    {
        if (!ForecastBySymbol.TryGetValue(symbolId, out var forecast))
        {
            return null;
        }

        return forecast.CreatedAt <= asOfDate ? forecast : null;
    }

    /// <summary>
    ///     Gets the total daily trade volume for a symbol on a given date.
    ///     Returns 0 if no aggregate data is available for that date.
    /// </summary>
    public decimal GetDailyVolume(Id<Symbol> symbolId, DateTimeOffset date)
    {
        var dateOnly = DateOnly.FromDateTime(date.UtcDateTime);

        if (!AggregatesBySymbol.TryGetValue(symbolId, out var aggregates))
        {
            return 0m;
        }

        var aggregate = aggregates.Find(a => a.Date == dateOnly);
        return aggregate?.TotalVolume ?? 0m;
    }
}
