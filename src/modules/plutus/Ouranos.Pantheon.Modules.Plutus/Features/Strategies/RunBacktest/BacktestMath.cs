using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public static class BacktestMath
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

    public static (decimal NetProceeds, decimal ExitVolume, decimal NetPnl) ComputeExit(
        OpenPosition pos,
        decimal exitPrice,
        decimal taxRate,
        Market market,
        decimal dayVolume,
        decimal fillCap,
        decimal slippageMultiplier
    )
    {
        var exitVolume = Math.Min(pos.Volume, fillCap);

        if (exitVolume <= 0 || exitPrice <= 0)
        {
            return (0m, 0m, 0m);
        }

        var slippage = ComputeSlippage(exitVolume, dayVolume, slippageMultiplier);
        var adjustedExitPrice = exitPrice * (1 - slippage);

        var grossExitValue = adjustedExitPrice * exitVolume;
        var netProceeds = grossExitValue - ComputeTax(grossExitValue, taxRate, market);
        var costBasis = pos.EntryPrice * exitVolume;
        var netPnl = netProceeds - costBasis;

        return (netProceeds, exitVolume, netPnl);
    }

    public static decimal ComputeTax(decimal grossValue, decimal taxRate, Market market)
    {
        var taxCap = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        return Math.Min(grossValue * taxRate, taxCap);
    }

    /// <summary>
    ///     Records a (possibly partial) exit against an open position. Partial exits accumulate
    ///     on the position so each round trip yields exactly one closed position once the
    ///     remaining volume reaches zero.
    /// </summary>
    public static (OpenPosition? Remaining, BacktestPosition? Closed) ApplyExit(
        OpenPosition pos,
        decimal exitPrice,
        decimal exitVolume,
        decimal netPnl,
        DateTimeOffset exitTime
    )
    {
        var accumulated = pos with
        {
            Volume = pos.Volume - exitVolume,
            ExitedVolume = pos.ExitedVolume + exitVolume,
            ExitedGross = pos.ExitedGross + (exitPrice * exitVolume),
            RealizedPnl = pos.RealizedPnl + netPnl,
        };

        if (accumulated.Volume > 0)
        {
            return (accumulated, null);
        }

        var averageExitPrice =
            accumulated.ExitedVolume > 0 ? accumulated.ExitedGross / accumulated.ExitedVolume : 0m;

        return (
            null,
            CreateClosedPosition(
                accumulated,
                averageExitPrice,
                accumulated.ExitedVolume,
                accumulated.RealizedPnl,
                exitTime
            )
        );
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

    public static BacktestPosition CreateClosedPosition(
        OpenPosition pos,
        decimal exitPrice,
        decimal exitVolume,
        decimal profitLoss,
        DateTimeOffset exitTime
    )
    {
        var returnPercent = pos.EntryPrice > 0 ? profitLoss / (pos.EntryPrice * exitVolume) : 0;

        return new BacktestPosition
        {
            SymbolId = pos.SymbolId.ToString(),
            SymbolName = pos.SymbolName,
            EntryPrice = pos.EntryPrice,
            ExitPrice = exitPrice,
            Volume = exitVolume,
            ProfitLoss = profitLoss,
            ReturnPercent = returnPercent,
            EntryTime = pos.EntryTime,
            ExitTime = exitTime,
        };
    }
}
