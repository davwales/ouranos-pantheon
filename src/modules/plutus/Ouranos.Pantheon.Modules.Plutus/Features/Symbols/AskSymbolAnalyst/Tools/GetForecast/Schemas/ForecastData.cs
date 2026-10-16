namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetForecast.Schemas;

public sealed record ForecastData(
    ForecastPriceData Baseline,
    IReadOnlyList<ForecastDayData> Days,
    ForecastErrorData? Error
);
