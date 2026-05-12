using System.Net.Http.Json;
using System.Runtime.CompilerServices;
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
    private readonly OpenAIClient _openAiClient;
    private readonly ILogger<OuranosMachineLearningClient> _logger;

    public OuranosMachineLearningClient(
        ILogger<OuranosMachineLearningClient> logger,
        HttpClient httpClient,
        OpenAIClient openAiClient
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(httpClient);
        Guard.Against.Null(openAiClient);

        _logger = logger;
        _httpClient = httpClient;
        _openAiClient = openAiClient;
    }

    public async IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(
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
        cancellationToken.ThrowIfCancellationRequested();

        var chatClient = _openAiClient.GetChatClient(model);
        var chatMessages = messages.Select(MapMessage).ToList();
        var options = BuildOptions(temperature, maxTokens, frequencyPenalty);

        ChatTokenUsage? usage = null;

        await foreach (
            var update in chatClient.CompleteChatStreamingAsync(
                chatMessages,
                options,
                cancellationToken
            )
        )
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    yield return new ChatCompletionChunk(part.Text, null);
                }
            }

            if (update.Usage is not null)
            {
                usage = update.Usage;
            }
        }

        if (usage is not null)
        {
            yield return new ChatCompletionChunk(
                null,
                new ChatCompletionUsage(
                    usage.InputTokenCount,
                    usage.OutputTokenCount,
                    usage.TotalTokenCount
                )
            );
        }

        _logger.LogDebug("Successfully streamed chat completion using model '{Model}'.", model);
    }

    public async Task<ChatCompletionResult> GenerateChatCompletionAsync(
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

        var chatClient = _openAiClient.GetChatClient(model);
        var chatMessages = messages.Select(MapMessage).ToList();
        var options = BuildOptions(temperature, maxTokens, frequencyPenalty);

        var result = await chatClient.CompleteChatAsync(chatMessages, options, cancellationToken);
        var content = result.Value.Content[0].Text;

        ChatCompletionUsage? usage = null;
        if (result.Value.Usage is not null)
        {
            usage = new ChatCompletionUsage(
                result.Value.Usage.InputTokenCount,
                result.Value.Usage.OutputTokenCount,
                result.Value.Usage.TotalTokenCount
            );
        }

        _logger.LogDebug("Successfully completed chat using model '{Model}'.", model);
        return new ChatCompletionResult(content, usage);
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

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, "plutus/forecast");
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<List<List<ForecastPoint>>>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to parse plutus forecast response.");

        _logger.LogDebug("Successfully generated plutus forecasts using Ouranos ML.");
        return result;
    }

    private static ChatMessage MapMessage(MessageDto message) =>
        message.Role switch
        {
            RoleDto.System => ChatMessage.CreateSystemMessage(message.Content),
            RoleDto.User => ChatMessage.CreateUserMessage(message.Content),
            RoleDto.Assistant => ChatMessage.CreateAssistantMessage(message.Content),
            _ => throw new InvalidOperationException($"Unknown role: {message.Role}"),
        };

    private static ChatCompletionOptions BuildOptions(
        float? temperature,
        int? maxTokens,
        float? frequencyPenalty
    )
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
