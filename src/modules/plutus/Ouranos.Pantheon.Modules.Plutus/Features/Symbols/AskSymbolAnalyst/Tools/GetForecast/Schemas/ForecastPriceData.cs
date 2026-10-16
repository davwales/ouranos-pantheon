namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetForecast.Schemas;

public sealed record ForecastPriceData(
    decimal AveragePrice,
    decimal LowPrice,
    decimal HighPrice,
    decimal Volume
);
