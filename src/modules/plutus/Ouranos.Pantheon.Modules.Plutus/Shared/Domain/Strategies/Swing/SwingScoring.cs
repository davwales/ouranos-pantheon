using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>
///     Turns reconstructed signals into a swing score. Each input is its signal blended 70/30 with
///     the average of that signal over the previous <see cref="SignalHistoryWindowSize" /> eligible
///     days of the run, so a one-day spike moves the score less than a sustained move.
/// </summary>
public static class SwingScoring
{
    public const int SignalHistoryWindowSize = 6;

    private const decimal TrendBlendWeight = 0.3m;

    public static SignalType SignalOf(InputKind kind)
    {
        return kind switch
        {
            InputKind.SignalTaxAdjustedRoi => SignalType.TaxAdjustedRoi,
            InputKind.SignalVolumeAnomaly => SignalType.VolumeAnomaly,
            InputKind.SignalTrendMomentum => SignalType.TrendMomentum,
            InputKind.SignalBollingerBands => SignalType.BollingerBands,
            InputKind.SignalRsi => SignalType.Rsi,
            InputKind.SignalMovingAverageCrossover => SignalType.MovingAverageCrossover,
            InputKind.SignalPriceVelocity => SignalType.PriceVelocity,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    /// <summary>
    ///     Input scores for every day from <paramref name="first" /> to <paramref name="last" />.
    ///     Signal history starts empty at <paramref name="first" />, as it does for a run starting
    ///     that day.
    /// </summary>
    public static IReadOnlyDictionary<
        DateOnly,
        IReadOnlyDictionary<Id<Symbol>, SwingInputs>
    > ScoreInputs(SwingSignalTable table, DateOnly first, DateOnly last)
    {
        var history = new Dictionary<Id<Symbol>, Dictionary<SignalType, List<decimal>>>();
        var days = new Dictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingInputs>>();

        for (var day = first; day <= last; day = day.AddDays(1))
        {
            var symbols = table.On(day);
            var scored = new Dictionary<Id<Symbol>, SwingInputs>(symbols.Count);

            foreach (var (symbolId, signals) in symbols)
            {
                history.TryGetValue(symbolId, out var symbolHistory);
                scored[symbolId] = new SwingInputs(
                    signals.Price,
                    signals.IsEligible,
                    Blend(signals.Signals, symbolHistory)
                );
            }

            foreach (var (symbolId, signals) in symbols)
            {
                if (signals.IsEligible)
                {
                    AppendHistory(history, symbolId, signals.Signals);
                }
            }

            days[day] = scored;
        }

        return days;
    }

    /// <summary>
    ///     Weighted average of the available input scores, normalized by total absolute weight so a
    ///     negative weight inverts an input instead of flipping the whole score. Null when no
    ///     weighted input has a score.
    /// </summary>
    public static decimal? Combine(
        IReadOnlyDictionary<InputKind, decimal> inputScores,
        IReadOnlyList<InputWeight> inputWeights
    )
    {
        var weightedSum = 0m;
        var totalWeight = 0m;

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

    private static Dictionary<InputKind, decimal> Blend(
        IReadOnlyDictionary<SignalType, decimal> signals,
        Dictionary<SignalType, List<decimal>>? history
    )
    {
        var scores = new Dictionary<InputKind, decimal>();
        foreach (var kind in Enum.GetValues<InputKind>())
        {
            if (!signals.TryGetValue(SignalOf(kind), out var latest) || latest == 0m)
            {
                continue;
            }

            if (
                history is not { Count: > 0 }
                || !history.TryGetValue(SignalOf(kind), out var series)
                || series.Count == 0
            )
            {
                scores[kind] = latest;
                continue;
            }

            scores[kind] =
                ((1m - TrendBlendWeight) * latest) + (TrendBlendWeight * series.Average());
        }

        return scores;
    }

    private static void AppendHistory(
        Dictionary<Id<Symbol>, Dictionary<SignalType, List<decimal>>> history,
        Id<Symbol> symbolId,
        IReadOnlyDictionary<SignalType, decimal> signals
    )
    {
        if (signals.Count == 0)
        {
            return;
        }

        if (!history.TryGetValue(symbolId, out var byType))
        {
            byType = [];
            history[symbolId] = byType;
        }

        foreach (var (type, value) in signals)
        {
            if (!byType.TryGetValue(type, out var series))
            {
                series = [];
                byType[type] = series;
            }

            series.Add(value);
            if (series.Count > SignalHistoryWindowSize)
            {
                series.RemoveAt(0);
            }
        }
    }
}
