using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy.Schemas;

public sealed record GetStrategyResponse(
    Id<Strategy> Id,
    Id<Market> MarketId,
    string Name,
    string? Description,
    TradingConfiguration TradingConfiguration,
    List<InputWeight> InputWeights,
    InputThresholds Thresholds,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
