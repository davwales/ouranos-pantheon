namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;

public sealed record GetMarketForecastPredictionResponse(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume,
    decimal Margin,
    decimal TotalMargin,
    decimal PriceChange,
    decimal MinPriceChange,
    decimal MaxPriceChange,
    decimal VolumeChange,
    decimal TotalValueChange
);
