using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record BacktestParameters(
    Id<Market> MarketId,
    Strategy Strategy,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m,
    TradingConfiguration? ConfigurationOverride = null,
    SignalWeightedConfig? SignalWeightedConfigOverride = null,
    ForecastMomentumConfig? ForecastMomentumConfigOverride = null,
    MeanReversionConfig? MeanReversionConfigOverride = null,
    RecipeArbitrageConfig? RecipeArbitrageConfigOverride = null
)
{
    public TradingConfiguration Configuration =>
        ConfigurationOverride ?? Strategy.TradingConfiguration;

    public SignalWeightedConfig? SignalWeightedConfig =>
        SignalWeightedConfigOverride ?? Strategy.SignalWeightedConfig;

    public ForecastMomentumConfig? ForecastMomentumConfig =>
        ForecastMomentumConfigOverride ?? Strategy.ForecastMomentumConfig;

    public MeanReversionConfig? MeanReversionConfig =>
        MeanReversionConfigOverride ?? Strategy.MeanReversionConfig;

    public RecipeArbitrageConfig? RecipeArbitrageConfig =>
        RecipeArbitrageConfigOverride ?? Strategy.RecipeArbitrageConfig;

    public int TotalDays => (int)(EndDate - StartDate).TotalDays;
}
