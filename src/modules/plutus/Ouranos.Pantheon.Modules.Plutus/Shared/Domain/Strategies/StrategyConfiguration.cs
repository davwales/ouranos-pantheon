namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record StrategyConfiguration(
    List<SignalWeight>? SignalWeights = null,
    decimal? BuyThreshold = null,
    decimal? SellThreshold = null,
    decimal? ForecastMovementThreshold = null,
    int? ForecastHorizonDays = null,
    decimal? DeviationMultiplier = null,
    int? MeanTimeFrameValue = null,
    decimal? MinMarginPercent = null,
    List<CompositeComponent>? Components = null,
    int? MaxPositions = null,
    decimal? MaxPositionPercent = null,
    int? HoldPeriodDays = null
)
{
    private StrategyConfiguration() : this(null, null)
    {
    }
}