namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;

public interface IStrategyExecutor
{
    StrategyType SupportedType { get; }

    decimal? Score(StrategyScoreContext context, TradingConfiguration configuration);
}