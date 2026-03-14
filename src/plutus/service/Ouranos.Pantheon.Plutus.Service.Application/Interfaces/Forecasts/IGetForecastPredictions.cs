using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;

public interface IGetForecastPredictions
{
    Task<List<List<ForecastPoint>>> GetPredictionsAsync(
        List<List<ForecastPoint>> historicalData,
        CancellationToken cancellationToken = default
    );
}