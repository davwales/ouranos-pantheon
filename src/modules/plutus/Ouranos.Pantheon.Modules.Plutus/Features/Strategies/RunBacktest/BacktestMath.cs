using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public static class BacktestMath
{
    public static (decimal NetProceeds, decimal ExitVolume, decimal NetPnl) ComputeExit(
        OpenPosition pos,
        decimal exitPrice,
        decimal taxRate,
        Market market,
        decimal dailyVolume,
        decimal volumeParticipationRate,
        decimal slippageMultiplier
    )
    {
        var maxSellableVolume =
            dailyVolume > 0 ? Math.Floor(dailyVolume * volumeParticipationRate) : pos.Volume;

        var exitVolume = Math.Min(pos.Volume, maxSellableVolume);

        if (exitVolume <= 0)
        {
            return (0m, 0m, 0m);
        }

        var volumeImpact = dailyVolume > 0 ? exitVolume / dailyVolume : 0m;
        var slippage = volumeImpact * slippageMultiplier;
        var adjustedExitPrice = exitPrice * (1 - slippage);

        var grossExitValue = adjustedExitPrice * exitVolume;
        var taxAmount = grossExitValue * taxRate;
        var taxCap = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var cappedTax = Math.Min(taxAmount, taxCap);

        var netProceeds = grossExitValue - cappedTax;
        var costBasis = pos.EntryPrice * exitVolume;
        var netPnl = netProceeds - costBasis;

        return (netProceeds, exitVolume, netPnl);
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
