namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;

public sealed record ForecastModelRequest(
    int NumPredictions,
    List<List<ForecastModelPoint>> Points
);
