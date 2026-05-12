namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public enum StrategyType
{
    SignalWeighted = 1,
    ForecastMomentum = 2,
    MeanReversion = 3,
    RecipeArbitrage = 4,
    Composite = 5,
}
