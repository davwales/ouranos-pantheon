using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Service.Aphrodite.Infra.OuranosMl.Requests;

namespace Ouranos.Pantheon.Service.Aphrodite.Infra.OuranosMl;

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
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(httpClient);

        _logger = logger;
        _httpClient = httpClient;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
        _logger.LogTrace("Attempting to send generate completion request with payload '{@payload}' to Ouranos ML.",
            payload);

        var jsonBody = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, "generation/text")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _logger.LogTrace("Read chunk: {Chunk}", chunk);

            if (!string.IsNullOrWhiteSpace(chunk)) yield return chunk;
        }

        _logger.LogDebug("Successfully generated completion using Ouranos ML.");
    }
}