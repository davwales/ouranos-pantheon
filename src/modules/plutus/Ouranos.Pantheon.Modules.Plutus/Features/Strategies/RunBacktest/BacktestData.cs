using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

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
    public List<Signal> Signals { get; }

    /// <summary>
    ///     Daily closing prices indexed by (SymbolId, Date).
    ///     Replaces GetClosePrice's linear scan over all trades.
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

    /// <summary>
    ///     Signals indexed by SymbolId for O(1) lookup.
    /// </summary>
    public Dictionary<Id<Symbol>, List<Signal>> SignalsBySymbol { get; }

    private BacktestData(
        Market market,
        List<Symbol> symbols,
        Dictionary<Id<Symbol>, List<MarketTradeSnapshot>> snapshotsBySymbol,
        Dictionary<Id<Symbol>, Forecast> forecastBySymbol,
        Dictionary<Id<Symbol>, List<Signal>> signalsBySymbol,
        Dictionary<(Id<Symbol> SymbolId, DateOnly Date), decimal> dailyPricesByDate,
        Dictionary<Id<Symbol>, List<DailyTradeAggregate>> aggregatesBySymbol
    )
    {
        Market = market;
        Symbols = symbols;
        SnapshotsBySymbol = snapshotsBySymbol;
        ForecastBySymbol = forecastBySymbol;
        SignalsBySymbol = signalsBySymbol;
        DailyPricesByDate = dailyPricesByDate;
        AggregatesBySymbol = aggregatesBySymbol;

        // Keep flat lists for backward compatibility with methods that iterate all items
        Snapshots = snapshotsBySymbol.SelectMany(kvp => kvp.Value).ToList();
        Forecasts = forecastBySymbol.Values.ToList();
        Signals = signalsBySymbol.SelectMany(kvp => kvp.Value).ToList();
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
        List<Signal> signals,
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

        var signalsBySymbol = signals
            .GroupBy(s => s.SymbolId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var dailyPricesByDate = dailyPrices
            .ToDictionary(dp => (dp.SymbolId, dp.Date), dp => dp.ClosePrice);

        var aggregatesBySymbol = dailyAggregates
            .GroupBy(a => a.SymbolId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.Date).ToList());

        return new BacktestData(
            market,
            symbols,
            snapshotsBySymbol,
            forecastBySymbol,
            signalsBySymbol,
            dailyPricesByDate,
            aggregatesBySymbol
        );
    }

    /// <summary>
    ///     Gets the closing price for a symbol on or before a given date.
    ///     Uses the pre-built daily price index for O(1) lookup instead of
    ///     scanning all trades.
    /// </summary>
    public decimal GetClosePrice(Id<Symbol> symbolId, DateTimeOffset date)
    {
        var dateOnly = DateOnly.FromDateTime(date.UtcDateTime);

        // Try exact date match first (most common case)
        if (DailyPricesByDate.TryGetValue((symbolId, dateOnly), out var price))
        {
            return price;
        }

        // Search backwards up to 7 days for a price on a nearby date
        // (handles weekends, holidays, days with no trades)
        for (var i = 1; i <= 7; i++)
        {
            if (DailyPricesByDate.TryGetValue((symbolId, dateOnly.AddDays(-i)), out var nearbyPrice))
            {
                return nearbyPrice;
            }
        }

        return 0m;
    }

    /// <summary>
    ///     Gets the most recent closing price for a symbol before or on a given date.
    ///     Uses binary search on the sorted daily aggregate list.
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
    ///     Gets snapshots for a symbol, returning short/medium/long timeframes.
    ///     Uses the pre-built index for O(1) lookup.
    /// </summary>
    public (MarketTradeSnapshot? Short, MarketTradeSnapshot? Medium, MarketTradeSnapshot? Long)
        GetSnapshotsForSymbol(Id<Symbol> symbolId)
    {
        if (!SnapshotsBySymbol.TryGetValue(symbolId, out var symbolSnaps))
        {
            return (null, null, null);
        }

        return (
            symbolSnaps.FirstOrDefault(s => s.TimeFrame == TimeFrame.OneHour),
            symbolSnaps.FirstOrDefault(s => s.TimeFrame == TimeFrame.OneWeek),
            symbolSnaps.FirstOrDefault(s => s.TimeFrame == TimeFrame.OneMonth)
        );
    }

    /// <summary>
    ///     Gets signals for a symbol. Uses the pre-built index for O(1) lookup.
    /// </summary>
    public List<Signal> GetSignalsForSymbol(Id<Symbol> symbolId)
    {
        return SignalsBySymbol.TryGetValue(symbolId, out var symbolSignals) ? symbolSignals : [];
    }

    /// <summary>
    ///     Gets the forecast for a symbol. Uses the pre-built index for O(1) lookup.
    /// </summary>
    public Forecast? GetForecastForSymbol(Id<Symbol> symbolId)
    {
        return ForecastBySymbol.TryGetValue(symbolId, out var forecast) ? forecast : null;
    }
}