namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;

public sealed record ForecastModelPoint(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);
