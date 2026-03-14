namespace Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

public record ForecastPoint(
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
)
{
    protected ForecastPoint() : this(0, 0, 0, 0)
    {
    }
}