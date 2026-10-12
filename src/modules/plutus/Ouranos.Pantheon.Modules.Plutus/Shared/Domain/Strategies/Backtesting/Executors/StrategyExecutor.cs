using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

/// <summary>
///     Strategy executor that blends per-input scores from all registered
///     <see cref="IInputScorer" /> implementations using the strategy's
///     <see cref="InputWeight" /> vector. Weights are relative and may be negative; the
///     blend is normalized by total absolute weight and clamped to [-1, 1].
/// </summary>
public sealed class StrategyExecutor(IEnumerable<IInputScorer> scorers) : IStrategyExecutor
{
    private readonly Dictionary<InputKind, IInputScorer> _scorers = Guard
        .Against.Null(scorers)
        .GroupBy(s => s.Kind)
        .ToDictionary(g => g.Key, g => g.First());

    public decimal? Score(StrategyScoreContext context, TradingConfiguration configuration)
    {
        return Combine(ScoreInputs(context), context.InputWeights);
    }

    public IReadOnlyDictionary<InputKind, decimal> ScoreInputs(StrategyScoreContext context)
    {
        var scores = new Dictionary<InputKind, decimal>();
        foreach (var (kind, scorer) in _scorers)
        {
            var score = scorer.Score(context);
            if (score.HasValue)
            {
                scores[kind] = score.Value;
            }
        }

        return scores;
    }

    /// <summary>
    ///     Weighted average of the available input scores, normalized by the total absolute
    ///     weight so negative weights invert an input instead of flipping the whole score.
    /// </summary>
    public decimal? Combine(
        IReadOnlyDictionary<InputKind, decimal> inputScores,
        IReadOnlyList<InputWeight> inputWeights
    )
    {
        decimal weightedSum = 0m;
        decimal totalWeight = 0m;

        foreach (var weight in inputWeights)
        {
            if (weight.Weight == 0m || !inputScores.TryGetValue(weight.Kind, out var score))
            {
                continue;
            }

            weightedSum += score * weight.Weight;
            totalWeight += Math.Abs(weight.Weight);
        }

        if (totalWeight == 0m)
        {
            return null;
        }

        return Math.Clamp(weightedSum / totalWeight, -1m, 1m);
    }
}
