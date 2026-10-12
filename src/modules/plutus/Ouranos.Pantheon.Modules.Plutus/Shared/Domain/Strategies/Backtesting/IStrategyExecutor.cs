using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;

public interface IStrategyExecutor
{
    decimal? Score(StrategyScoreContext context, TradingConfiguration configuration);

    /// <summary>
    ///     Per-input scores for every registered scorer that produced a value. Independent of
    ///     the strategy's weights, so it can be computed once and recombined for many weightings.
    /// </summary>
    IReadOnlyDictionary<InputKind, decimal> ScoreInputs(StrategyScoreContext context);

    decimal? Combine(
        IReadOnlyDictionary<InputKind, decimal> inputScores,
        IReadOnlyList<InputWeight> inputWeights
    );
}
