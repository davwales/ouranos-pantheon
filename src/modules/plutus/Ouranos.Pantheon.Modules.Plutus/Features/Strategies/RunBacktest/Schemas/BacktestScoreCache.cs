using System.Collections.Concurrent;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

/// <summary>
///     Per-input scores for every (symbol, day) a backtest scored. Signals and their history
///     blend depend only on market data and the run's date range, never on the strategy's
///     weights, thresholds or sizing, so optimization candidates over the same range share
///     one cache: the first run fills it and every later run only recombines weights.
/// </summary>
public sealed class BacktestScoreCache
{
    private readonly ConcurrentDictionary<
        DateOnly,
        IReadOnlyDictionary<Id<Symbol>, CachedSymbolScore>
    > _days = new();

    public bool TryGetDay(
        DateOnly day,
        out IReadOnlyDictionary<Id<Symbol>, CachedSymbolScore> scores
    )
    {
        if (_days.TryGetValue(day, out var cached))
        {
            scores = cached;
            return true;
        }

        scores = new Dictionary<Id<Symbol>, CachedSymbolScore>();
        return false;
    }

    public void SetDay(DateOnly day, IReadOnlyDictionary<Id<Symbol>, CachedSymbolScore> scores)
    {
        _days.TryAdd(day, scores);
    }
}
