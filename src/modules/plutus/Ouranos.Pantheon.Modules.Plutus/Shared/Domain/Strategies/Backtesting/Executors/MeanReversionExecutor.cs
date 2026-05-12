namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

public sealed class MeanReversionExecutor : IStrategyExecutor
{
    public StrategyType SupportedType => StrategyType.MeanReversion;

    public decimal? Score(StrategyScoreContext context, TradingConfiguration configuration)
    {
        if (context.PriceBuckets.Count < 2 || context.CurrentPrice == 0)
        {
            return null;
        }

        var meanReversionConfig = context.MeanReversionConfig;
        var timeFrame = meanReversionConfig?.MeanTimeFrameValue ?? context.PriceBuckets.Count;
        var effectiveBuckets = Math.Min(timeFrame, context.PriceBuckets.Count);

        var prices = context
            .PriceBuckets.Skip(context.PriceBuckets.Count - effectiveBuckets)
            .Select(b => b.AveragePrice)
            .ToList();

        var mean = prices.Average();
        var stdDev = (decimal)Math.Sqrt((double)prices.Average(p => (p - mean) * (p - mean)));

        if (stdDev == 0)
        {
            return 0m;
        }

        var deviationMultiplier = meanReversionConfig?.DeviationMultiplier ?? 2m;
        if (deviationMultiplier == 0)
        {
            deviationMultiplier = 2m;
        }

        var score = (mean - context.CurrentPrice) / (stdDev * deviationMultiplier);

        return Math.Clamp(score, -1m, 1m);
    }
}
