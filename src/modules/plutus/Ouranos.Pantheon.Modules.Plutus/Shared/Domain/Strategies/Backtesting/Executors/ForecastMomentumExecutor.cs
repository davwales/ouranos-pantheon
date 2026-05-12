namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

public sealed class ForecastMomentumExecutor : IStrategyExecutor
{
    public StrategyType SupportedType => StrategyType.ForecastMomentum;

    public decimal? Score(StrategyScoreContext context, TradingConfiguration configuration)
    {
        if (context.ForecastedPriceChange is null || context.CurrentPrice == 0)
        {
            return null;
        }

        var threshold = context.ForecastMomentumConfig?.ForecastMovementThreshold ?? 0.01m;
        if (threshold == 0)
        {
            threshold = 0.01m;
        }

        var change = context.ForecastedPriceChange.Value;
        var score = change / (threshold * 3m);

        return Math.Clamp(score, -1m, 1m);
    }
}