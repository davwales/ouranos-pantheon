using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy.Schemas;

public sealed record CreateStrategyInput(
    Id<Market> MarketId,
    string Name,
    string? Description,
    TradingConfiguration Configuration,
    List<InputWeight> InputWeights,
    InputThresholds? Thresholds = null
);
