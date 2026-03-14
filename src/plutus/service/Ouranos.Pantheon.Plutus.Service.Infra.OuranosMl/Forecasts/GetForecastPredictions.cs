using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Options;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Infra.OuranosMl.Forecasts;

public sealed class GetForecastPredictions : IGetForecastPredictions
{
    private readonly IOuranosMachineLearningClient _client;
    private readonly IOptions<ForecastingOptions> _forecastingOptions;
    private readonly ILogger<GetForecastPredictions> _logger;

    public GetForecastPredictions(
        ILogger<GetForecastPredictions> logger,
        IOuranosMachineLearningClient client,
        IOptions<ForecastingOptions> forecastingOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(client);
        Guard.Against.Null(forecastingOptions);

        _logger = logger;
        _client = client;
        _forecastingOptions = forecastingOptions;
    }

    public async Task<List<List<ForecastPoint>>> GetPredictionsAsync(
        List<List<ForecastPoint>> historicalData,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to get forecast predictions from Ouranos ml with historical data '{@historicalData}'.",
            historicalData
        );
        cancellationToken.ThrowIfCancellationRequested();

        var request = new GetPlutusForecastsRequest(
            _forecastingOptions.Value.NumPredictions,
            historicalData.Select(item =>
                item.Select(x => new Core.Infra.OuranosMl.Dtos.ForecastPoint(
                        x.AveragePrice,
                        x.MinPrice,
                        x.MaxPrice,
                        x.Volume
                    )
                ).ToList()
            ).ToList()
        );

        var forecasts = await _client.GetPlutusForecasts(request, cancellationToken);
        var response = forecasts.Select(f =>
            f.Select(x => new ForecastPoint(
                    x.AveragePrice,
                    x.MinPrice,
                    x.MaxPrice,
                    x.Volume
                )
            ).ToList()
        ).ToList();

        _logger.LogInformation("Successfully retrieved forecast predictions from Ouranos ml.");
        return response;
    }
}