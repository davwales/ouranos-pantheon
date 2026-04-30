using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record CompositeComponent(Id<Strategy> StrategyId, StrategyType Type, decimal Weight)
{
    private CompositeComponent() : this(new Id<Strategy>(Guid.NewGuid().ToString()), default, 0)
    {
    }
}