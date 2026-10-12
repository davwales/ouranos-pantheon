using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

public static class SwingMath
{
    /// <summary>Upper bound on modelled price impact so a thin day cannot produce absurd fills.</summary>
    public const decimal MaxSlippage = 0.05m;

    /// <summary>
    ///     Square-root market impact of trading <paramref name="volume" /> units against a day
    ///     that traded <paramref name="dayVolume" /> units, capped at <see cref="MaxSlippage" />.
    /// </summary>
    public static decimal ComputeSlippage(
        decimal volume,
        decimal dayVolume,
        decimal slippageMultiplier
    )
    {
        if (volume <= 0 || dayVolume <= 0)
        {
            return 0m;
        }

        var impact = (decimal)Math.Sqrt((double)(volume / dayVolume)) * slippageMultiplier;
        return Math.Min(impact, MaxSlippage);
    }

    /// <summary>
    ///     Sells up to <paramref name="fillCap" /> of <paramref name="heldVolume" /> at the day's
    ///     average price less slippage and the market's sales tax.
    /// </summary>
    public static (decimal NetProceeds, decimal Volume) ComputeExit(
        decimal heldVolume,
        decimal exitPrice,
        MarketRules rules,
        decimal dayVolume,
        decimal fillCap,
        decimal slippageMultiplier
    )
    {
        var exitVolume = Math.Min(heldVolume, fillCap);

        if (exitVolume <= 0 || exitPrice <= 0)
        {
            return (0m, 0m);
        }

        var slippage = ComputeSlippage(exitVolume, dayVolume, slippageMultiplier);
        return (rules.NetSale(exitPrice * (1 - slippage), exitVolume), exitVolume);
    }

    public static IReadOnlyList<PriceBucket> BuildPriceBucketsFromAggregates(
        List<DailyTradeAggregate> aggregates,
        int priceBucketCount = 25
    )
    {
        if (aggregates.Count == 0)
        {
            return [];
        }

        var bucketSize = Math.Max(1, aggregates.Count / priceBucketCount);
        var buckets = new List<PriceBucket>();

        for (var i = 0; i < aggregates.Count; i += bucketSize)
        {
            var remaining = Math.Min(bucketSize, aggregates.Count - i);
            var chunk = aggregates.GetRange(i, remaining);
            var totalVolume = chunk.Sum(a => a.TotalVolume);

            var weightedAvgPrice =
                totalVolume > 0
                    ? chunk.Sum(a => a.AveragePrice * a.TotalVolume) / totalVolume
                    : chunk.Average(a => a.AveragePrice);

            buckets.Add(
                new PriceBucket(
                    chunk[0].Date.ToDateTime(TimeOnly.MinValue),
                    weightedAvgPrice,
                    chunk.Min(a => a.MinPrice),
                    chunk.Max(a => a.MaxPrice),
                    totalVolume
                )
            );
        }

        return buckets;
    }
}
