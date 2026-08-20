using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;

public sealed record UpdateStrategyInput(
    Id<Strategy> StrategyId,
    string Name,
    string? Description,
    TradingConfiguration Configuration,
    List<InputWeight> InputWeights,
    InputThresholds? Thresholds = null
);
