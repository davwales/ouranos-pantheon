namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

public sealed record ForecastingOptions(
    bool IsEnabled,
    int NumPredictions,
    int HistoryDays,
    int BatchSize,
    string ModelName
)
{
    public ForecastingOptions()
        : this(
            IsEnabled: true,
            NumPredictions: 7,
            HistoryDays: 30,
            BatchSize: 500,
            ModelName: "plutus-forecasting-v1"
        ) { }
}
