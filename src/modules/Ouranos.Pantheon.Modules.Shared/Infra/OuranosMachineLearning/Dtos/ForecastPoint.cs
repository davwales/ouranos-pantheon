namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

public sealed record ForecastPoint(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);
