using System.Net.Http.Json;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Responses;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;
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

    public async IAsyncEnumerable<ResponseStreamChunk> StreamResponseAsync(
        string model,
        string instructions,
        IReadOnlyList<ResponseInputItemDto> input,
        IReadOnlyList<FunctionToolDto> tools,
        float? temperature = null,
        int? maxTokens = null,
        ReasoningEffort? reasoningEffort = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to stream response using model '{Model}' with {Count} input items and {ToolCount} tools.",
            model,
            input.Count,
            tools.Count
        );
        cancellationToken.ThrowIfCancellationRequested();

        var options = BuildResponseOptions(
            model,
            instructions,
            input.Select(MapResponseItem),
            temperature,
            maxTokens,
            reasoningEffort
        );
        options.StreamingEnabled = true;

        foreach (var tool in tools)
        {
            options.Tools.Add(
                ResponseTool.CreateFunctionTool(
                    functionName: tool.Name,
                    functionParameters: tool.Parameters,
                    strictModeEnabled: true,
                    functionDescription: tool.Description
                )
            );
        }

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
                case StreamingResponseOutputItemDoneUpdate { Item: FunctionCallResponseItem call }:
                    yield return new ResponseStreamChunk(
                        null,
                        null,
                        null,
                        new FunctionCallDto(
                            call.CallId,
                            call.FunctionName,
                            call.FunctionArguments.ToString()
                        )
                    );
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

    public async Task<GeneratedResponse> GenerateResponseAsync(
        string model,
        string instructions,
        IReadOnlyList<ResponseInputItemDto> input,
        float? temperature = null,
        int? maxTokens = null,
        ReasoningEffort? reasoningEffort = null,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to generate response using model '{Model}' with {Count} input items.",
            model,
            input.Count
        );
        cancellationToken.ThrowIfCancellationRequested();

        var options = BuildResponseOptions(
            model,
            instructions,
            input.Select(MapResponseItem),
            temperature,
            maxTokens,
            reasoningEffort
        );

        var result = await _openAiClient
            .GetResponsesClient()
            .CreateResponseAsync(options, cancellationToken);

        _logger.LogDebug("Successfully generated response using model '{Model}'.", model);
        return new GeneratedResponse(
            result.Value.GetOutputText() ?? string.Empty,
            result.Value.Usage is { } usage ? MapUsage(usage) : null
        );
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
            input.Select(MapMessageItem),
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
        return StructuredCompletionParser.Parse<T>(result.Value.GetOutputText(), _logger);
    }

    public async IAsyncEnumerable<ImageGenerationEventDto> StreamImageGenerationAsync(
        GenerateImageRequest payload,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to generate a {Width}x{Height} image using model '{Model}'.",
            payload.Width,
            payload.Height,
            payload.Model
        );
        cancellationToken.ThrowIfCancellationRequested();

        await foreach (
            var imageEvent in StreamImageEventsAsync(
                "images/generations",
                ImageGenerationMapping.ToContent(payload),
                cancellationToken
            )
        )
        {
            yield return imageEvent;
        }

        _logger.LogDebug("Successfully generated an image using Ouranos ML.");
    }

    public async IAsyncEnumerable<ImageGenerationEventDto> StreamImageEditAsync(
        EditImageRequest payload,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to edit an image at strength {Strength} using model '{Model}'.",
            payload.Strength,
            payload.Model
        );
        cancellationToken.ThrowIfCancellationRequested();

        await foreach (
            var imageEvent in StreamImageEventsAsync(
                "images/edits",
                ImageGenerationMapping.ToContent(payload),
                cancellationToken
            )
        )
        {
            yield return imageEvent;
        }

        _logger.LogDebug("Successfully edited an image using Ouranos ML.");
    }

    // Generations and edits stream the same events, ending with done.
    private async IAsyncEnumerable<ImageGenerationEventDto> StreamImageEventsAsync(
        string path,
        HttpContent content,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Content = content;

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            throw await ImageGenerationMapping.ToExceptionAsync(response, cancellationToken);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var isDone = false;
        await foreach (var item in SseParser.Create(stream).EnumerateAsync(cancellationToken))
        {
            var imageEvent = ImageGenerationMapping.ToEvent(item.EventType, item.Data);
            if (imageEvent is null)
            {
                continue;
            }

            yield return imageEvent;

            if (imageEvent is ImageGenerationDoneDto)
            {
                isDone = true;
                break;
            }
        }

        if (!isDone)
        {
            throw new OuranosMlRequestException(
                "incomplete_stream",
                "The image stream ended before the image was finished."
            );
        }
    }

    public async Task<ImageModelsDto> GetImageModelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to fetch image models from Ouranos ML.");
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await _httpClient.GetAsync("images/models", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await ImageGenerationMapping.ToImageModelsAsync(
            response.Content,
            cancellationToken
        );

        _logger.LogDebug(
            "Successfully fetched {Count} image models from Ouranos ML.",
            result.Models.Count
        );
        return result;
    }

    public async Task<List<ImageLoraDto>> GetImageLorasAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to fetch image LoRAs from Ouranos ML.");
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await _httpClient.GetAsync("images/loras", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await ImageGenerationMapping.ToImageLorasAsync(
            response.Content,
            cancellationToken
        );

        _logger.LogDebug("Successfully fetched {Count} image LoRAs from Ouranos ML.", result.Count);
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

    private static ResponseItem MapResponseItem(ResponseInputItemDto item)
    {
        return item switch
        {
            MessageInputItemDto message => MapMessageItem(message.Message),
            FunctionCallInputItemDto function => ResponseItem.CreateFunctionCallItem(
                function.Call.CallId,
                function.Call.Name,
                BinaryData.FromString(function.Call.Arguments)
            ),
            FunctionCallOutputInputItemDto output => ResponseItem.CreateFunctionCallOutputItem(
                output.CallId,
                output.Output
            ),
            _ => throw new InvalidOperationException($"Unknown response input item: {item}"),
        };
    }

    private static ResponseItem MapMessageItem(MessageDto message)
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
        IEnumerable<ResponseItem> input,
        float? temperature,
        int? maxTokens,
        ReasoningEffort? reasoningEffort
    )
    {
        var options = new CreateResponseOptions(model, input)
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
}
