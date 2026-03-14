namespace Ouranos.Pantheon.Plutus.Service.Application.Options;

public sealed record ForecastingOptions(
    bool IsEnabled,
    int BatchSize,
    int NumPredictions,
    int SequenceLength
)
{
    public const string SectionName = "Ouranos:Plutus:Forecasting";

    public ForecastingOptions() : this(true, 100, 7, 30)
    {
    }
}