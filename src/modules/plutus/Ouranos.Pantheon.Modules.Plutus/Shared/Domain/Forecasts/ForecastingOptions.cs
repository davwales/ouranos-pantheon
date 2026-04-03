namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

public sealed record ForecastingOptions(
    bool IsEnabled,
    bool RemoveOutdatedForecasts,
    int NumPredictions,
    int HistoryDays,
    int BatchSize
)
{
    public ForecastingOptions() : this(
        IsEnabled: true,
        RemoveOutdatedForecasts: true,
        NumPredictions: 7,
        HistoryDays: 30,
        BatchSize: 500
    )
    {
    }
}