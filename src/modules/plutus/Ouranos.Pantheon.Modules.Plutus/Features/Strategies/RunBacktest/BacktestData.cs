using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

/// <summary>
///     Pre-indexed backtest data optimized for O(1) lookups in the hot loop.
///     All collections are indexed by SymbolId.
/// </summary>
public sealed class BacktestData
{
    /// <summary>OneHour snapshots are approximated from 1 day of daily aggregates (no sub-daily data available).</summary>
    private const int ShortSnapshotDays = 1;

    private const int MediumSnapshotDays = 7;

    private const int LongSnapshotDays = 30;

    private const decimal MinutesPerDay = 1440m;

    public Market Market { get; }
    public List<Symbol> Symbols { get; }

    public Dictionary<Id<Symbol>, List<DailyTradeAggregate>> AggregatesBySymbol { get; }

    private readonly Dictionary<Id<Symbol>, Symbol> _symbolsById;

    private BacktestData(
        Market market,
        List<Symbol> symbols,
        Dictionary<Id<Symbol>, List<DailyTradeAggregate>> aggregatesBySymbol
    )
    {
        Market = market;
        Symbols = symbols;
        AggregatesBySymbol = aggregatesBySymbol;
        _symbolsById = symbols.ToDictionary(s => s.Id);
    }

    /// <summary>
    ///     Creates <see cref="BacktestData" /> from raw query results,
    ///     indexing all collections by SymbolId for O(1) lookups.
    /// </summary>
    public static BacktestData FromRaw(
        Market market,
        List<Symbol> symbols,
        List<DailyTradeAggregate> dailyAggregates
    )
    {
        var aggregatesBySymbol = dailyAggregates
            .GroupBy(a => a.SymbolId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.Date).ToList());

        return new BacktestData(market, symbols, aggregatesBySymbol);
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

        var idx = FindLastIndex(aggregates, a => a.Date <= dateOnly);
        if (idx < 0)
        {
            return 0m;
        }

        return aggregates[idx].AveragePrice;
    }

    public List<DailyTradeAggregate> GetWindowAggregates(
        Id<Symbol> symbolId,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        int? windowDays = null
    )
    {
        if (!AggregatesBySymbol.TryGetValue(symbolId, out var aggregates))
        {
            return [];
        }

        var endDate = DateOnly.FromDateTime(windowEnd.UtcDateTime);

        if (windowDays.HasValue)
        {
            var startDate = DateOnly
                .FromDateTime(windowEnd.UtcDateTime)
                .AddDays(-(windowDays.Value - 1));
            return SliceAggregates(aggregates, startDate, endDate);
        }

        var legacyStartDate = DateOnly.FromDateTime(windowStart.UtcDateTime);
        return SliceAggregates(aggregates, legacyStartDate, endDate);
    }

    public (
        MarketTradeSnapshot? Short,
        MarketTradeSnapshot? Medium,
        MarketTradeSnapshot? Long
    ) GetSnapshotsForSymbol(Id<Symbol> symbolId, DateTimeOffset asOfDate)
    {
        var shortSnap = ReconstructSnapshot(
            symbolId,
            TimeFrame.OneHour,
            ShortSnapshotDays,
            asOfDate
        );
        var mediumSnap = ReconstructSnapshot(
            symbolId,
            TimeFrame.OneWeek,
            MediumSnapshotDays,
            asOfDate
        );
        var longSnap = ReconstructSnapshot(
            symbolId,
            TimeFrame.OneMonth,
            LongSnapshotDays,
            asOfDate
        );

        return (shortSnap, mediumSnap, longSnap);
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

        var idx = FindFirstIndex(aggregates, a => a.Date >= dateOnly);
        return idx >= 0 && aggregates[idx].Date == dateOnly ? aggregates[idx].TotalVolume : 0m;
    }

    /// <summary>
    ///     Reconstructs a snapshot by aggregating daily trades over the trailing
    ///     window; scales volumes to the target timeframe.
    /// </summary>
    private MarketTradeSnapshot? ReconstructSnapshot(
        Id<Symbol> symbolId,
        TimeFrame timeframe,
        int windowDays,
        DateTimeOffset asOfDate
    )
    {
        if (!AggregatesBySymbol.TryGetValue(symbolId, out var aggregates) || aggregates.Count == 0)
        {
            return null;
        }

        var asOfDateOnly = DateOnly.FromDateTime(asOfDate.UtcDateTime);
        var startDate = asOfDateOnly.AddDays(-(windowDays - 1));
        var window = SliceAggregates(aggregates, startDate, asOfDateOnly);
        if (window.Count == 0)
        {
            return null;
        }

        var totalVolume = window.Sum(a => a.TotalVolume);
        var totalSpent = window.Sum(a => a.AveragePrice * a.TotalVolume);
        var minPrice = window.Min(a => a.MinPrice);
        var maxPrice = window.Max(a => a.MaxPrice);

        var timeframeSpan = timeframe.ToTimeSpan();
        if (timeframeSpan is null)
        {
            return null;
        }

        var timeframeMinutes = (decimal)timeframeSpan.Value.TotalMinutes;
        var actualWindowMinutes = window.Count * MinutesPerDay;
        var volumeScale = actualWindowMinutes > 0 ? timeframeMinutes / actualWindowMinutes : 1m;

        var scaledVolume = totalVolume * volumeScale;
        var scaledSpent = totalSpent * volumeScale;

        var taxRate = Market.Taxes.Flat?.Rate ?? 0m;
        var tax = maxPrice * taxRate;

        _symbolsById.TryGetValue(symbolId, out var symbol);
        var limit = symbol?.AdditionalFields.Limit ?? totalVolume;

        // Reconstructed snapshots carry no real transaction count; pass 0 so
        // NumTransactions isn't mistaken for one.
        return new MarketTradeSnapshot(
            Market.Id,
            symbolId,
            timeframe,
            scaledSpent,
            minPrice,
            maxPrice,
            scaledVolume,
            0,
            limit,
            tax
        );
    }

    private static List<DailyTradeAggregate> SliceAggregates(
        List<DailyTradeAggregate> aggregates,
        DateOnly startDate,
        DateOnly endDate
    )
    {
        if (aggregates.Count == 0 || startDate > endDate)
        {
            return [];
        }

        var startIdx = FindFirstIndex(aggregates, a => a.Date >= startDate);
        if (startIdx < 0)
        {
            return [];
        }

        var endIdx = FindLastIndex(aggregates, a => a.Date <= endDate);
        if (endIdx < 0 || endIdx < startIdx)
        {
            return [];
        }

        var count = endIdx - startIdx + 1;
        return aggregates.GetRange(startIdx, count);
    }

    private static int FindFirstIndex<T>(IList<T> list, Func<T, bool> predicate)
    {
        var lo = 0;
        var hi = list.Count - 1;
        var result = -1;

        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;
            if (predicate(list[mid]))
            {
                result = mid;
                hi = mid - 1;
            }
            else
            {
                lo = mid + 1;
            }
        }

        return result;
    }

    private static int FindLastIndex<T>(IList<T> list, Func<T, bool> predicate)
    {
        var lo = 0;
        var hi = list.Count - 1;
        var result = -1;

        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;
            if (predicate(list[mid]))
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return result;
    }
}
