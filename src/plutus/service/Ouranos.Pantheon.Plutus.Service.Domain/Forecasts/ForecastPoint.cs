namespace Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

public sealed record ForecastPoint(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);