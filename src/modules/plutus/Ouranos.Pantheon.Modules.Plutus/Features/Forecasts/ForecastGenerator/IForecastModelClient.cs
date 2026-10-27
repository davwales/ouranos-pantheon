using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator;

/// <summary>
/// OuranosMl's price forecasting model (<c>POST /plutus/forecast</c>).
/// </summary>
public interface IForecastModelClient
{
    /// <summary>
    /// Predicts the next points of each history, returned in the same order.
    /// </summary>
    Task<List<List<ForecastModelPoint>>> ForecastAsync(
        ForecastModelRequest request,
        CancellationToken cancellationToken = default
    );
}
