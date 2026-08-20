namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;

public interface IStrategyExecutor
{
    decimal? Score(StrategyScoreContext context, TradingConfiguration configuration);
}
