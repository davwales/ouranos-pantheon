using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

/// <param name="Recommendations">Orders the strategy would place now.</param>
/// <param name="VolumeParticipationRate">The participation rate the orders were sized with.</param>
public sealed record GetRecommendationsResponse(
    IReadOnlyList<StrategyRecommendation> Recommendations,
    decimal VolumeParticipationRate
);
