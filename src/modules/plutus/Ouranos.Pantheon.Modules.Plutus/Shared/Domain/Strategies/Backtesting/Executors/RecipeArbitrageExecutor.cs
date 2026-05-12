namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

public sealed class RecipeArbitrageExecutor : IStrategyExecutor
{
    public StrategyType SupportedType => StrategyType.RecipeArbitrage;

    public decimal? Score(StrategyScoreContext context, TradingConfiguration configuration)
    {
        var shortSnap = context.ShortSnapshot;
        if (shortSnap is null || shortSnap.MinPrice == 0)
        {
            return null;
        }

        var minMargin = context.RecipeArbitrageConfig?.MinMarginPercent ?? 0.01m;
        if (minMargin == 0)
        {
            minMargin = 0.01m;
        }

        var roi = (shortSnap.MaxPrice - shortSnap.MinPrice - shortSnap.Tax) / shortSnap.MinPrice;

        if (roi < minMargin)
        {
            return null;
        }

        var score = roi / (minMargin * 3m);

        return Math.Clamp(score, 0m, 1m);
    }
}