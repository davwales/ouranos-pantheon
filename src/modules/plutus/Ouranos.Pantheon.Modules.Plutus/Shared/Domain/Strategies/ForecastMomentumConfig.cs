namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record ForecastMomentumConfig(
    decimal? ForecastMovementThreshold = null,
    int? ForecastHorizonDays = null
)
{
    public ForecastMomentumConfig()
        : this(null, null) { }
}
