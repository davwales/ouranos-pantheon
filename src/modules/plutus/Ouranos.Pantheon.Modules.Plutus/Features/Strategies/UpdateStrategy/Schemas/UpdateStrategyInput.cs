using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;

public sealed record UpdateStrategyInput(
    Id<Strategy> StrategyId,
    string Name,
    string? Description,
    StrategyConfiguration Configuration
);
