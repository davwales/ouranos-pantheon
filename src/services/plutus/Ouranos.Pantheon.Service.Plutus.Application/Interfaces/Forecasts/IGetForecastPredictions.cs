using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;

public interface IGetForecastPredictions
{
    Task<List<List<ForecastPoint>>> GetPredictionsAsync(
        List<List<ForecastPoint>> historicalData,
        CancellationToken cancellationToken = default
    );
}