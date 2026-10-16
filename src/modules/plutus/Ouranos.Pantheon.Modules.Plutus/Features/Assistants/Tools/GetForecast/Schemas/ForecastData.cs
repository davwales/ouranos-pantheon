namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecast.Schemas;

public sealed record ForecastData(
    ForecastPriceData Baseline,
    IReadOnlyList<ForecastDayData> Days,
    ForecastErrorData? Error
);
