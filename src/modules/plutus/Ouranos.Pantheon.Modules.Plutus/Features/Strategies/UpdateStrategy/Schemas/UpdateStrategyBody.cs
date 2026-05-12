using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;

public sealed record UpdateStrategyBody(
    string Name,
    string? Description,
    TradingConfiguration Configuration,
    SignalWeightedConfig? SignalWeightedConfig = null,
    ForecastMomentumConfig? ForecastMomentumConfig = null,
    MeanReversionConfig? MeanReversionConfig = null,
    RecipeArbitrageConfig? RecipeArbitrageConfig = null
);