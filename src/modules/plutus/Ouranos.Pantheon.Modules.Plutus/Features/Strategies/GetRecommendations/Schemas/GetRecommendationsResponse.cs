using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

public sealed record GetRecommendationsResponse(
    IReadOnlyList<StrategyRecommendation> Recommendations
);
