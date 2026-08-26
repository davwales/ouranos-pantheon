using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;

/// <summary>
///     Base class for the seven signal-computer input scorers. Reads the signal value
///     of its <see cref="SignalType" /> from <c>context.Signals</c> and optionally blends
///     it 70/30 with the average of its signal history (the trend component). The blend
///     is applied per-signal here rather than to the weighted average; the math is
///     equivalent by linearity of the downstream weighted average.
/// </summary>
public abstract class SignalInputScorerBase : IInputScorer
{
    /// <summary>
    ///     Blend weight for the trend (history average) component: 0.3 = 70% latest, 30% trend.
    /// </summary>
    private const decimal TrendBlendWeight = 0.3m;

    public abstract InputKind Kind { get; }

    public abstract SignalType SignalType { get; }

    public decimal? Score(StrategyScoreContext context)
    {
        var signal = context.Signals.FirstOrDefault(s => s.Type == SignalType);
        if (signal is null || signal.Value == 0m)
        {
            return null;
        }

        var latestScore = signal.Value;

        if (context.SignalHistoryByType is not { Count: > 0 } historyByType)
        {
            return latestScore;
        }

        if (!historyByType.TryGetValue(SignalType, out var history) || history.Count == 0)
        {
            return latestScore;
        }

        var trendScore = history.Average();
        return (1m - TrendBlendWeight) * latestScore + TrendBlendWeight * trendScore;
    }
}
