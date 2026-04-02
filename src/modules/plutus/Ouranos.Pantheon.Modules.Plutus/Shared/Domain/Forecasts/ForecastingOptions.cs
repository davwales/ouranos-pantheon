namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

public sealed record ForecastingOptions(
    bool IsEnabled,
    bool RemoveOutdatedForecasts
)
{
    public ForecastingOptions() : this(IsEnabled: true, RemoveOutdatedForecasts: true)
    {
    }
}