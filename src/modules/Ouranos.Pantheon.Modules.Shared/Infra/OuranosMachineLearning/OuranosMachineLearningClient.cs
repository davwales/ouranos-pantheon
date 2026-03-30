using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningClient : IOuranosMachineLearningClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIClient _openAIClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<OuranosMachineLearningClient> _logger;

    public OuranosMachineLearningClient(
        ILogger<OuranosMachineLearningClient> logger,
        HttpClient httpClient,
        OpenAIClient openAIClient
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(httpClient);
        Guard.Against.Null(openAIClient);

        _logger = logger;
        _httpClient = httpClient;
        _openAIClient = openAIClient;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    public async IAsyncEnumerable<string> StreamChatCompletionAsync(
        string model,
        List<MessageDto> messages,
        float? temperature = null,
        int? maxTokens = null,
        float? frequencyPenalty = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to stream chat completion using model '{Model}' with {Count} messages.",
            model,
            messages.Count
        );

        var chatClient = _openAIClient.GetChatClient(model);
        var chatMessages = messages.Select(MapMessage).ToList();
        var options = BuildOptions(temperature, maxTokens, frequencyPenalty);

        await foreach (var update in chatClient.CompleteChatStreamingAsync(chatMessages, options, cancellationToken))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    yield return part.Text;
                }
            }
        }

        _logger.LogDebug("Successfully streamed chat completion using model '{Model}'.", model);
    }

    public async Task<string> GenerateChatCompletionAsync(
        string model,
        List<MessageDto> messages,
        float? temperature = null,
        int? maxTokens = null,
        float? frequencyPenalty = null,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to complete chat using model '{Model}' with {Count} messages.",
            model,
            messages.Count
        );

        var chatClient = _openAIClient.GetChatClient(model);
        var chatMessages = messages.Select(MapMessage).ToList();
        var options = BuildOptions(temperature, maxTokens, frequencyPenalty);

        var result = await chatClient.CompleteChatAsync(chatMessages, options, cancellationToken);
        var content = result.Value.Content[0].Text;

        _logger.LogDebug("Successfully completed chat using model '{Model}'.", model);
        return content;
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
            Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<List<ForecastPoint>>>(cancellationToken) ??
                     throw new InvalidOperationException("Failed to parse plutus forecast response.");

        _logger.LogDebug("Successfully generated plutus forecasts using Ouranos ML.");
        return result;
    }

    private static ChatMessage MapMessage(MessageDto message) => message.Role switch
    {
        RoleDto.System => ChatMessage.CreateSystemMessage(message.Content),
        RoleDto.User => ChatMessage.CreateUserMessage(message.Content),
        RoleDto.Assistant => ChatMessage.CreateAssistantMessage(message.Content),
        _ => throw new InvalidOperationException($"Unknown role: {message.Role}")
    };

    private static ChatCompletionOptions BuildOptions(float? temperature, int? maxTokens, float? frequencyPenalty)
    {
        var options = new ChatCompletionOptions();

        if (temperature.HasValue)
        {
            options.Temperature = temperature.Value;
        }

        if (maxTokens.HasValue)
        {
            options.MaxOutputTokenCount = maxTokens.Value;
        }

        if (frequencyPenalty.HasValue)
        {
            options.FrequencyPenalty = frequencyPenalty.Value;
        }

        return options;
    }
}
