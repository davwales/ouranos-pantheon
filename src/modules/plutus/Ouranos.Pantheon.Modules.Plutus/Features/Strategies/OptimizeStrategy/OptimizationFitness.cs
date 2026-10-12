using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;

internal static class OptimizationFitness
{
    private const double UnderTradingPenaltyPerMissingTrade = 0.01;

    private const decimal MaxSymbolProfitShare = 0.25m;

    private const double DaysPerMonth = 30.0;

    /// <summary>
    ///     Return after discounting any single symbol's profit beyond
    ///     <see cref="MaxSymbolProfitShare" /> of gross profit, minus a drawdown penalty and a
    ///     penalty for trading fewer than <paramref name="minTrades" /> round trips. A strategy
    ///     that depends on one lucky item scores as if that item had earned only its fair share.
    /// </summary>
    public static double Compute(
        BacktestResults results,
        IReadOnlyList<BacktestPosition> positions,
        decimal budget,
        double drawdownWeight,
        int minTrades
    )
    {
        var concentrationAdjustedReturn = (double)ConcentrationAdjustedReturn(positions, budget);
        var tradeShortfall = Math.Max(0, minTrades - results.TotalTrades);

        return concentrationAdjustedReturn
            - (drawdownWeight * (double)results.MaxDrawdownPercent)
            - (tradeShortfall * UnderTradingPenaltyPerMissingTrade);
    }

    /// <summary>Scales a monthly trade target to a segment, keeping at least one trade when any were asked for.</summary>
    public static int MinTradesForSegment(int minTradesPerMonth, int segmentDays)
    {
        if (minTradesPerMonth <= 0)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Round(minTradesPerMonth * segmentDays / DaysPerMonth));
    }

    public static decimal ConcentrationAdjustedReturn(
        IReadOnlyList<BacktestPosition> positions,
        decimal budget
    )
    {
        if (budget <= 0 || positions.Count == 0)
        {
            return 0m;
        }

        var bySymbol = positions
            .GroupBy(p => p.SymbolId)
            .Select(g => g.Sum(p => p.ProfitLoss))
            .ToList();

        var grossProfit = bySymbol.Where(p => p > 0).Sum();
        var excess = Math.Max(0m, bySymbol.Max() - (grossProfit * MaxSymbolProfitShare));
        return (bySymbol.Sum() - excess) / budget;
    }

    /// <summary>
    ///     Blends the mean with the worst segment, so a configuration has to hold up in every
    ///     sub-period of the in-sample window rather than earn its score in a single stretch.
    /// </summary>
    public static double CombineSegments(IReadOnlyList<double> segmentFitness)
    {
        return (0.75 * segmentFitness.Average()) + (0.25 * segmentFitness.Min());
    }

    /// <summary>Splits the window into consecutive segments of roughly <paramref name="segmentDays" /> whole days.</summary>
    public static IReadOnlyList<FitnessSegment> BuildSegments(
        DateTimeOffset start,
        DateTimeOffset end,
        int segmentDays
    )
    {
        var totalDays = Math.Max(1, (int)Math.Round((end - start).TotalDays));
        var count = Math.Max(1, totalDays / segmentDays);
        var segments = new List<FitnessSegment>(count);

        for (var i = 0; i < count; i++)
        {
            var segmentStart = start.AddDays(totalDays * i / count);
            var segmentEnd = i == count - 1 ? end : start.AddDays(totalDays * (i + 1) / count);
            segments.Add(new FitnessSegment(segmentStart, segmentEnd));
        }

        return segments;
    }

    public static DateTimeOffset InSampleEnd(
        DateTimeOffset start,
        DateTimeOffset end,
        double outSampleRatio
    )
    {
        var ratio = Math.Clamp(outSampleRatio, 0.05, 0.5);
        return start.AddDays(Math.Round((end - start).TotalDays * (1.0 - ratio)));
    }

    public static bool IsValidated(BacktestResults inSample, BacktestResults outSample)
    {
        return inSample.TotalReturn > 0m && outSample.TotalReturn > 0m;
    }
}
