using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record CachedSymbolScore(
    decimal Price,
    IReadOnlyDictionary<InputKind, decimal> InputScores
);
