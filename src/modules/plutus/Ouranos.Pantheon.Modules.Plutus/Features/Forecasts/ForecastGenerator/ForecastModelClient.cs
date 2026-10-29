using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator;

public sealed class ForecastModelClient(ILogger<ForecastModelClient> logger, HttpClient httpClient)
    : IForecastModelClient
{
    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly ILogger<ForecastModelClient> _logger = Guard.Against.Null(logger);
    private readonly HttpClient _httpClient = Guard.Against.Null(httpClient);

    public async Task<List<List<ForecastModelPoint>>> ForecastAsync(
        ForecastModelRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to forecast {count} symbols using Ouranos ML.",
            request.Points.Count
        );
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await _httpClient.PostAsJsonAsync(
            "plutus/forecast",
            request,
            SnakeCaseOptions,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<List<List<ForecastModelPoint>>>(
                SnakeCaseOptions,
                cancellationToken
            ) ?? throw new InvalidOperationException("Failed to parse plutus forecast response.");

        _logger.LogDebug("Successfully forecast {count} symbols using Ouranos ML.", result.Count);
        return result;
    }
}
