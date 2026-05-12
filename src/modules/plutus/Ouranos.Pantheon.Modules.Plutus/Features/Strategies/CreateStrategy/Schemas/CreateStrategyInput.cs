using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy.Schemas;

public sealed record CreateStrategyInput(
    Id<Market> MarketId,
    string Name,
    string? Description,
    StrategyType Type,
    TradingConfiguration Configuration,
    SignalWeightedConfig? SignalWeightedConfig = null,
    ForecastMomentumConfig? ForecastMomentumConfig = null,
    MeanReversionConfig? MeanReversionConfig = null,
    RecipeArbitrageConfig? RecipeArbitrageConfig = null
);
