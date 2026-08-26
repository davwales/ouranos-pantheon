using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;

/// <summary>
///     Strategy executor that blends per-input scores from all registered
///     <see cref="IInputScorer" /> implementations using the strategy's
///     <see cref="InputWeight" /> vector. Weights are relative; the blend is
///     normalized by total weight at score time and clamped to [-1, 1].
/// </summary>
public sealed class StrategyExecutor(IEnumerable<IInputScorer> scorers) : IStrategyExecutor
{
    private readonly Dictionary<InputKind, IInputScorer> _scorers = Guard
        .Against.Null(scorers)
        .GroupBy(s => s.Kind)
        .ToDictionary(g => g.Key, g => g.First());

    public decimal? Score(StrategyScoreContext context, TradingConfiguration configuration)
    {
        var weightMap = context
            .InputWeights.Where(w => w.Weight != 0m)
            .ToDictionary(w => w.Kind, w => w.Weight);

        if (weightMap.Count == 0)
        {
            return null;
        }

        decimal weightedSum = 0m;
        decimal totalWeight = 0m;

        foreach (var (kind, weight) in weightMap)
        {
            if (!_scorers.TryGetValue(kind, out var scorer))
            {
                continue;
            }

            var subScore = scorer.Score(context);
            if (subScore is null)
            {
                continue;
            }

            weightedSum += subScore.Value * weight;
            totalWeight += weight;
        }

        if (totalWeight == 0m)
        {
            return null;
        }

        return Math.Clamp(weightedSum / totalWeight, -1m, 1m);
    }
}
