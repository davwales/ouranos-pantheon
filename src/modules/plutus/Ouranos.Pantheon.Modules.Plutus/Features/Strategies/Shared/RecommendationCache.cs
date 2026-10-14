using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

public static class RecommendationCache
{
    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(5);

    public static string TagFor(Id<Strategy> strategyId)
    {
        return $"plutus-recommendations:{strategyId.Value}";
    }
}
