namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecast.Schemas;

public sealed record ForecastDayData(
    int Day,
    decimal AveragePrice,
    decimal LowPrice,
    decimal HighPrice,
    decimal Margin
);
