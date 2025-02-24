namespace Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

public sealed record ForecastPoint(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);