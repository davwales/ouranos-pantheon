namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;

public sealed record ForecastPoint(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);