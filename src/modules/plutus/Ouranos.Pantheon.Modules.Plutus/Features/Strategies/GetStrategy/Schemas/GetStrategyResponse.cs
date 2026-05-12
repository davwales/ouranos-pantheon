using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy.Schemas;

public sealed record GetStrategyResponse(
    Id<Strategy> Id,
    Id<Market> MarketId,
    string Name,
    string? Description,
    StrategyType Type,
    TradingConfiguration TradingConfiguration,
    SignalWeightedConfig? SignalWeightedConfig,
    ForecastMomentumConfig? ForecastMomentumConfig,
    MeanReversionConfig? MeanReversionConfig,
    RecipeArbitrageConfig? RecipeArbitrageConfig,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
