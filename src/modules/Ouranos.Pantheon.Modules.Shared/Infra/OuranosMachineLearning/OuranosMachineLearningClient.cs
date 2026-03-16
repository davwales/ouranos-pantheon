using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningClient : IOuranosMachineLearningClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<OuranosMachineLearningClient> _logger;

    public OuranosMachineLearningClient(
        ILogger<OuranosMachineLearningClient> logger,
        HttpClient httpClient
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(httpClient);

        _logger = logger;
        _httpClient = httpClient;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    public async IAsyncEnumerable<string> GenerateCompletion(
        GenerateCompletionRequest payload,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to send generate completion request with payload '{@payload}' to Ouranos ML.",
            payload
        );

        var jsonBody = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, "generation/text")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _logger.LogTrace("Read chunk: {Chunk}", chunk);

            yield return chunk;
        }

        _logger.LogDebug("Successfully generated completion using Ouranos ML.");
    }

    public async IAsyncEnumerable<string> GenerateChatCompletion(
        GenerateChatCompletionRequest payload,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to send generate chat completion request with payload '{@payload}' to Ouranos ML.",
            payload
        );

        var jsonBody = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, "generation/chat")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _logger.LogTrace("Read chunk: {Chunk}", chunk);

            yield return chunk;
        }

        _logger.LogDebug("Successfully generated chat completion using Ouranos ML.");
    }

    public async Task<List<List<ForecastPoint>>> GetPlutusForecasts(
        GetPlutusForecastsRequest payload,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to generate plutus forecasts using Ouranos ML with payload '{@payload}'.",
            payload
        );
        cancellationToken.ThrowIfCancellationRequested();

        var jsonBody = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, "plutus/forecast")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<List<ForecastPoint>>>(cancellationToken) ??
                     throw new InvalidOperationException("Failed to parse plutus forecast response.");

        _logger.LogDebug("Successfully generated plutus forecasts using Ouranos ML.");
        return result;
    }
}