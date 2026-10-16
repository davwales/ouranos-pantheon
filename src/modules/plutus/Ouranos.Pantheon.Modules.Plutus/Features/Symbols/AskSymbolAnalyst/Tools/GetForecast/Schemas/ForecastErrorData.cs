namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetForecast.Schemas;

public sealed record ForecastErrorData(
    string ModelName,
    int WindowDays,
    int EvaluatedCount,
    IReadOnlyList<ForecastHorizonErrorData> Horizons,
    decimal? MeanForecastMinusActual
);
