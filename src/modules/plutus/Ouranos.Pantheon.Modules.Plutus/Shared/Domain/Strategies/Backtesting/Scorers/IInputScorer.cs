using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;

/// <summary>
///     Computes a normalized score in [-1, 1] for a single <see cref="InputKind" /> from a
///     <see cref="StrategyScoreContext" />. The strategy executor blends these per-input
///     scores using the strategy's <see cref="InputWeight" /> vector.
/// </summary>
public interface IInputScorer
{
    InputKind Kind { get; }

    decimal? Score(StrategyScoreContext context);
}
