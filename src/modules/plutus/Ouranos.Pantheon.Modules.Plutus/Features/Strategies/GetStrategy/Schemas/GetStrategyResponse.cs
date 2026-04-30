using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy.Schemas;

public sealed record GetStrategyResponse(
    Id<Strategy> Id,
    Id<Market> MarketId,
    string Name,
    string? Description,
    StrategyType Type,
    StrategyConfiguration Configuration,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);