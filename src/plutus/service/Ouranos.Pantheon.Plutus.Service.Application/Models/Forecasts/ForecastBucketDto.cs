namespace Ouranos.Pantheon.Plutus.Service.Application.Models.Forecasts;

public sealed record ForecastBucketDto(
    ForecastBucketIdDto Id,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);