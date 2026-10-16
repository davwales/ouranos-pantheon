namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecast.Schemas;

public sealed record ForecastPriceData(
    decimal AveragePrice,
    decimal LowPrice,
    decimal HighPrice,
    decimal Volume
);
