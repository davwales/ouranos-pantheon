using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

internal sealed record RecommendationCandidate(
    Symbol Symbol,
    decimal Score,
    decimal Price,
    string Rationale
);
