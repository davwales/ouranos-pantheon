using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>
///     Daily market history plus the signals reconstructed from it. Input scores depend only on
///     the data and the run's dates, never on weights, thresholds or sizing, so they are computed
///     once per date range and shared by every simulation over it.
/// </summary>
public sealed class SwingData(SwingMarketData market, SwingSignalTable signals)
    : IStrategyMarketData
{
    private readonly ConcurrentDictionary<
        (DateOnly First, DateOnly Last),
        Lazy<IReadOnlyDictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingInputs>>>
    > _inputs = new();

    public SwingMarketData Market { get; } = Guard.Against.Null(market);

    public SwingSignalTable Signals { get; } = Guard.Against.Null(signals);

    public IReadOnlyDictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingInputs>> Inputs(
        DateOnly first,
        DateOnly last
    )
    {
        return _inputs
            .GetOrAdd(
                (first, last),
                key => new Lazy<
                    IReadOnlyDictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingInputs>>
                >(() => SwingScoring.ScoreInputs(Signals, key.First, key.Last))
            )
            .Value;
    }
}
