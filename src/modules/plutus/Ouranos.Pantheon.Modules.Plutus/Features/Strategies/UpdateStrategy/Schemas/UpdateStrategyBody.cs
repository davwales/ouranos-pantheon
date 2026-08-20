using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;

public sealed record UpdateStrategyBody(
    string Name,
    string? Description,
    TradingConfiguration Configuration,
    List<InputWeight> InputWeights,
    InputThresholds? Thresholds = null
);
