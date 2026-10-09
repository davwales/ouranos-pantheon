using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

// The OpenAI SDK marks the whole Responses API as experimental; OuranosMl serves the stable
// subset of it that this client uses.
#pragma warning disable OPENAI001

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningClient : IOuranosMachineLearningClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIClient _openAiClient;
    private readonly ILogger<OuranosMachineLearningClient> _logger;

    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

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

        _logger.LogDebug("Successfully completed chat using model '{Model}'.", model);
        return MapResult(result.Value);
    }

    public async IAsyncEnumerable<ResponseStreamChunk> StreamResponseAsync(
        string model,
        string instructions,
        List<MessageDto> input,
        float? temperature = null,
        int? maxTokens = null,
        ReasoningEffort? reasoningEffort = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to stream response using model '{Model}' with {Count} input messages.",
            model,
            input.Count
        );
        cancellationToken.ThrowIfCancellationRequested();

        var options = BuildResponseOptions(
            model,
            instructions,
            input,
            temperature,
            maxTokens,
            reasoningEffort
        );
        options.StreamingEnabled = true;

        await foreach (
            var update in _openAiClient
                .GetResponsesClient()
                .CreateResponseStreamingAsync(options, cancellationToken)
        )
        {
            switch (update)
            {
                case StreamingResponseOutputTextDeltaUpdate { Delta.Length: > 0 } text:
                    yield return new ResponseStreamChunk(text.Delta, null, null);
                    break;
                case StreamingResponseReasoningSummaryTextDeltaUpdate { Delta.Length: > 0 } summary:
                    yield return new ResponseStreamChunk(null, summary.Delta, null);
                    break;
                case StreamingResponseReasoningTextDeltaUpdate { Delta.Length: > 0 } reasoning:
                    yield return new ResponseStreamChunk(null, reasoning.Delta, null);
                    break;
                case StreamingResponseCompletedUpdate { Response.Usage: { } usage }:
                    yield return new ResponseStreamChunk(null, null, MapUsage(usage));
                    break;
                case StreamingResponseIncompleteUpdate incomplete:
                    throw new InvalidOperationException(
                        $"Response was incomplete: '{incomplete.Response.IncompleteStatusDetails?.Reason}'."
                    );
                case StreamingResponseFailedUpdate failed:
                    throw new InvalidOperationException(
                        $"Response failed: '{failed.Response.Error?.Message}'."
                    );
                case StreamingResponseErrorUpdate error:
                    throw new InvalidOperationException(
                        $"Response stream errored: '{error.Message}'."
                    );
            }
        }

        _logger.LogDebug("Successfully streamed response using model '{Model}'.", model);
    }

    public async Task<T?> GenerateStructuredResponseAsync<T>(
        string model,
        string instructions,
        List<MessageDto> input,
        float? temperature = null,
        int? maxTokens = null,
        ReasoningEffort? reasoningEffort = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        _logger.LogTrace(
            "Attempting to generate structured response using model '{Model}' with {Count} input messages.",
            model,
            input.Count
        );
        cancellationToken.ThrowIfCancellationRequested();

        var options = BuildResponseOptions(
            model,
            instructions,
            input,
            temperature,
            maxTokens,
            reasoningEffort
        );

        options.TextOptions = new ResponseTextOptions
        {
            TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                typeof(T).Name,
                StructuredOutputSchema.For<T>(),
                jsonSchemaFormatDescription: null,
                jsonSchemaIsStrict: true
            ),
        };

        var result = await _openAiClient
            .GetResponsesClient()
            .CreateResponseAsync(options, cancellationToken);

        _logger.LogDebug(
            "Successfully generated structured response using model '{Model}'.",
            model
        );
        return StructuredCompletionParser.Parse<T>(result.Value.GetOutputText());
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

        var json = JsonSerializer.Serialize(payload, SnakeCaseOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, "plutus/forecast");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<List<List<ForecastPoint>>>(
                SnakeCaseOptions,
                cancellationToken
            ) ?? throw new InvalidOperationException("Failed to parse plutus forecast response.");

        _logger.LogDebug("Successfully generated plutus forecasts using Ouranos ML.");
        return result;
    }

    public async Task<List<AvailableModelDto>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to fetch available models from Ouranos ML.");
        cancellationToken.ThrowIfCancellationRequested();

        var models = await _openAiClient.GetOpenAIModelClient().GetModelsAsync(cancellationToken);

        var result = models
            .Value.Select(m => new AvailableModelDto(m.Id, m.OwnedBy ?? string.Empty))
            .ToList();

        _logger.LogDebug(
            "Successfully fetched {Count} available models from Ouranos ML.",
            result.Count
        );
        return result;
    }

    private static ChatMessage MapMessage(MessageDto message)
    {
        return message.Role switch
        {
            RoleDto.System => ChatMessage.CreateSystemMessage(message.Content),
            RoleDto.User => ChatMessage.CreateUserMessage(message.Content),
            RoleDto.Assistant => ChatMessage.CreateAssistantMessage(message.Content),
            _ => throw new InvalidOperationException($"Unknown role: {message.Role}"),
        };
    }

    private static ResponseItem MapResponseItem(MessageDto message)
    {
        return message.Role switch
        {
            RoleDto.System => ResponseItem.CreateSystemMessageItem(message.Content),
            RoleDto.User => ResponseItem.CreateUserMessageItem(message.Content),
            RoleDto.Assistant => ResponseItem.CreateAssistantMessageItem(message.Content),
            _ => throw new InvalidOperationException($"Unknown role: {message.Role}"),
        };
    }

    private static CreateResponseOptions BuildResponseOptions(
        string model,
        string instructions,
        List<MessageDto> input,
        float? temperature,
        int? maxTokens,
        ReasoningEffort? reasoningEffort
    )
    {
        var options = new CreateResponseOptions(model, input.Select(MapResponseItem))
        {
            Instructions = instructions,
            Temperature = temperature,
            MaxOutputTokenCount = maxTokens,
        };

        if (reasoningEffort.HasValue)
        {
            options.ReasoningOptions = new ResponseReasoningOptions
            {
                ReasoningEffortLevel = MapReasoningEffort(reasoningEffort.Value),
                ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Auto,
            };
        }

        return options;
    }

    private static ResponseReasoningEffortLevel MapReasoningEffort(ReasoningEffort effort)
    {
        return effort switch
        {
            ReasoningEffort.None => ResponseReasoningEffortLevel.None,
            ReasoningEffort.Low => ResponseReasoningEffortLevel.Low,
            ReasoningEffort.Medium => ResponseReasoningEffortLevel.Medium,
            ReasoningEffort.High => ResponseReasoningEffortLevel.High,
            _ => throw new ArgumentOutOfRangeException(nameof(effort), effort, null),
        };
    }

    private static ChatCompletionUsage MapUsage(ResponseTokenUsage usage)
    {
        return new ChatCompletionUsage(
            usage.InputTokenCount,
            usage.OutputTokenCount,
            usage.TotalTokenCount
        );
    }

    private static ChatCompletionResult MapResult(ChatCompletion completion)
    {
        var content = completion.Content.Count > 0 ? completion.Content[0].Text : string.Empty;

        ChatCompletionUsage? usage = null;
        if (completion.Usage is not null)
        {
            usage = new ChatCompletionUsage(
                completion.Usage.InputTokenCount,
                completion.Usage.OutputTokenCount,
                completion.Usage.TotalTokenCount
            );
        }

        return new ChatCompletionResult(content, usage);
    }

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
