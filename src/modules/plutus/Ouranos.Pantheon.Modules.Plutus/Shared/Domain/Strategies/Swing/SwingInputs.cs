using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>Per-input scores for one symbol on one decision day, independent of any weighting.</summary>
public sealed record SwingInputs(
    decimal Price,
    bool IsEligible,
    IReadOnlyDictionary<InputKind, decimal> Scores
);
