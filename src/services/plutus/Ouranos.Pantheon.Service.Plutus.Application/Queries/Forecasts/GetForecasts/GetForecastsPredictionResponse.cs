namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetForecasts;

public sealed record GetForecastsPredictionResponse(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume,
    decimal AveragePriceDelta,
    decimal MinPriceDelta,
    decimal MaxPriceDelta,
    decimal VolumeDelta,
    decimal GainDelta
);