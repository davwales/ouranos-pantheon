using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OpenAI;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;
using Ouranos.Pantheon.Modules.Shared.Contract.Tests.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.OuranosMachineLearning;

public sealed class OuranosMachineLearningClientTests
{
    private readonly ILogger<OuranosMachineLearningClient> _logger = Substitute.For<
        ILogger<OuranosMachineLearningClient>
    >();

    private readonly HttpClient _httpClient = new(Substitute.For<HttpMessageHandler>())
    {
        BaseAddress = new Uri("http://test.com/v1/"),
    };

    private readonly OpenAIClient _openAiClient = new(
        new System.ClientModel.ApiKeyCredential("test"),
        new OpenAIClientOptions { Endpoint = new Uri("http://test.com/v1/") }
    );

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new OuranosMachineLearningClient(null!, _httpClient, _openAiClient);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenHttpClientIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new OuranosMachineLearningClient(_logger, null!, _openAiClient);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenOpenAiClientIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new OuranosMachineLearningClient(_logger, _httpClient, null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task StreamResponseAsync_WhenStreamCompletes_ShouldYieldReasoningTextAndUsage()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                ("response.created", Lifecycle("response.created", 0, "in_progress", null)),
                (
                    "response.reasoning_summary_text.delta",
                    """{"type":"response.reasoning_summary_text.delta","sequence_number":1,"item_id":"rs_1","output_index":0,"summary_index":0,"delta":"Hmm"}"""
                ),
                (
                    "response.reasoning_text.delta",
                    """{"type":"response.reasoning_text.delta","sequence_number":2,"item_id":"rs_1","output_index":0,"content_index":0,"delta":" ok"}"""
                ),
                (
                    "response.output_text.delta",
                    """{"type":"response.output_text.delta","sequence_number":3,"item_id":"msg_1","output_index":1,"content_index":0,"delta":"Yes","logprobs":[]}"""
                ),
                (
                    "response.completed",
                    Lifecycle(
                        "response.completed",
                        4,
                        "completed",
                        """{"input_tokens":10,"input_tokens_details":{"cached_tokens":0},"output_tokens":5,"output_tokens_details":{"reasoning_tokens":2},"total_tokens":15}"""
                    )
                )
            )
        );
        var client = CreateClient(handler);

        // Act
        var chunks = new List<ResponseStreamChunk>();
        await foreach (
            var chunk in client.StreamResponseAsync(
                "test-model",
                "Be brief.",
                [
                    new MessageInputItemDto(new MessageDto("Hi", RoleDto.User)),
                    new MessageInputItemDto(new MessageDto("Hello", RoleDto.Assistant)),
                ],
                [],
                0.5f,
                256,
                ReasoningEffort.Medium
            )
        )
        {
            chunks.Add(chunk);
        }

        // Assert
        chunks.ShouldBe([
            new ResponseStreamChunk(null, "Hmm", null),
            new ResponseStreamChunk(null, " ok", null),
            new ResponseStreamChunk("Yes", null, null),
            new ResponseStreamChunk(null, null, new ChatCompletionUsage(10, 5, 15)),
        ]);
        var body = handler.RequestBody.ShouldNotBeNull();
        handler.RequestUri.ShouldNotBeNull().AbsolutePath.ShouldBe("/v1/responses");
        body["model"]!.GetValue<string>().ShouldBe("test-model");
        body["instructions"]!.GetValue<string>().ShouldBe("Be brief.");
        body["stream"]!.GetValue<bool>().ShouldBeTrue();
        body["max_output_tokens"]!.GetValue<int>().ShouldBe(256);
        body["reasoning"]!["effort"]!.GetValue<string>().ShouldBe("medium");
        body["input"]!.AsArray().Count.ShouldBe(2);
    }

    [Fact]
    public async Task StreamResponseAsync_WhenToolsAndToolItemsAreGiven_ShouldSendThemAndYieldFunctionCalls()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                (
                    "response.output_item.done",
                    """{"type":"response.output_item.done","sequence_number":0,"output_index":0,"item":{"type":"function_call","id":"fc_1","call_id":"call_2","name":"lookup","arguments":"{\"query\":\"salt\"}","status":"completed"}}"""
                ),
                ("response.completed", Lifecycle("response.completed", 1, "completed", null))
            )
        );
        var client = CreateClient(handler);

        // Act
        var chunks = new List<ResponseStreamChunk>();

        await foreach (
            var chunk in client.StreamResponseAsync(
                "test-model",
                "Be brief.",
                [
                    new MessageInputItemDto(new MessageDto("Hi", RoleDto.User)),
                    new FunctionCallInputItemDto(
                        new FunctionCallDto("call_1", "lookup", """{"query":"flour"}""")
                    ),
                    new FunctionCallOutputInputItemDto("call_1", "Bread flour."),
                ],
                [
                    new FunctionToolDto(
                        "lookup",
                        "Looks something up.",
                        BinaryData.FromString("""{"type":"object","properties":{}}""")
                    ),
                ]
            )
        )
        {
            chunks.Add(chunk);
        }

        // Assert
        chunks.ShouldBe([
            new ResponseStreamChunk(
                null,
                null,
                null,
                new FunctionCallDto("call_2", "lookup", """{"query":"salt"}""")
            ),
        ]);

        var body = handler.RequestBody.ShouldNotBeNull();
        var tool = body["tools"]!.AsArray().Single()!;
        tool["type"]!.GetValue<string>().ShouldBe("function");
        tool["name"]!.GetValue<string>().ShouldBe("lookup");
        tool["description"]!.GetValue<string>().ShouldBe("Looks something up.");
        tool["strict"]!.GetValue<bool>().ShouldBeTrue();

        var input = body["input"]!.AsArray();
        input[1]!["type"]!.GetValue<string>().ShouldBe("function_call");
        input[1]!["call_id"]!.GetValue<string>().ShouldBe("call_1");
        input[1]!["arguments"]!.GetValue<string>().ShouldBe("""{"query":"flour"}""");
        input[2]!["type"]!.GetValue<string>().ShouldBe("function_call_output");
        input[2]!["output"]!.GetValue<string>().ShouldBe("Bread flour.");
    }

    [Fact]
    public async Task StreamResponseAsync_WhenReasoningEffortIsNull_ShouldNotSendReasoning()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(("response.completed", Lifecycle("response.completed", 0, "completed", null)))
        );
        var client = CreateClient(handler);

        // Act
        await foreach (
            var _ in client.StreamResponseAsync(
                "test-model",
                "Be brief.",
                [new MessageInputItemDto(new MessageDto("Hi", RoleDto.User))],
                []
            )
        ) { }

        // Assert
        handler.RequestBody.ShouldNotBeNull().ContainsKey("reasoning").ShouldBeFalse();
    }

    [Theory]
    [InlineData("response.incomplete", "incomplete")]
    [InlineData("response.failed", "failed")]
    public async Task StreamResponseAsync_WhenResponseDoesNotComplete_ShouldThrow(
        string eventType,
        string status
    )
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse((eventType, Lifecycle(eventType, 0, status, null)))
        );
        var client = CreateClient(handler);

        // Act
        var act = async () =>
        {
            await foreach (
                var _ in client.StreamResponseAsync(
                    "test-model",
                    "Be brief.",
                    [new MessageInputItemDto(new MessageDto("Hi", RoleDto.User))],
                    []
                )
            ) { }
        };

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StreamResponseAsync_WhenStreamErrors_ShouldThrow()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                (
                    "error",
                    """{"type":"error","sequence_number":0,"code":"server_error","message":"The response stream from the LLM backend failed.","param":null}"""
                )
            )
        );
        var client = CreateClient(handler);

        // Act
        var act = async () =>
        {
            await foreach (
                var _ in client.StreamResponseAsync(
                    "test-model",
                    "Be brief.",
                    [new MessageInputItemDto(new MessageDto("Hi", RoleDto.User))],
                    []
                )
            ) { }
        };

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateResponseAsync_WhenCompleted_ShouldReturnTextAndUsageAndSendInput()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            ResponseJson(
                "completed",
                """[{"type":"message","id":"msg_1","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Cake night","annotations":[],"logprobs":[]}]}]""",
                """{"input_tokens":10,"input_tokens_details":{"cached_tokens":0},"output_tokens":5,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":15}"""
            )
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.GenerateResponseAsync(
            "test-model",
            "Name it.",
            [new MessageInputItemDto(new MessageDto("Let's bake", RoleDto.User))],
            0.2f,
            64,
            ReasoningEffort.None
        );

        // Assert
        result.ShouldBe(new GeneratedResponse("Cake night", new ChatCompletionUsage(10, 5, 15)));

        var body = handler.RequestBody.ShouldNotBeNull();
        body["instructions"]!.GetValue<string>().ShouldBe("Name it.");
        body["max_output_tokens"]!.GetValue<int>().ShouldBe(64);
        body["reasoning"]!["effort"]!.GetValue<string>().ShouldBe("none");
        body["input"]![0]!["role"]!.GetValue<string>().ShouldBe("user");
        body.ContainsKey("text").ShouldBeFalse();
    }

    [Fact]
    public async Task GenerateResponseAsync_WhenUsageIsMissing_ShouldReturnNullUsage()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            ResponseJson(
                "completed",
                """[{"type":"message","id":"msg_1","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Hi","annotations":[],"logprobs":[]}]}]""",
                null
            )
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.GenerateResponseAsync("test-model", "Say hi.", []);

        // Assert
        result.ShouldBe(new GeneratedResponse("Hi", null));
    }

    [Fact]
    public async Task GenerateStructuredResponseAsync_WhenOutputIsValidJson_ShouldParseAndSendJsonSchema()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            ResponseJson(
                "completed",
                """[{"type":"message","id":"msg_1","role":"assistant","status":"completed","content":[{"type":"output_text","text":"{\"title\":\"Cake\",\"description\":null,\"ingredients\":[],\"steps\":[\"Bake.\"]}","annotations":[],"logprobs":[]}]}]""",
                null
            )
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.GenerateStructuredResponseAsync<TestDocument>(
            "test-model",
            "Extract.",
            [new MessageDto("{}", RoleDto.User)],
            0f,
            1024,
            ReasoningEffort.Low
        );

        // Assert
        var document = result.ShouldNotBeNull();
        document.Title.ShouldBe("Cake");
        document.Steps.ShouldBe(["Bake."]);
        var body = handler.RequestBody.ShouldNotBeNull();
        body["text"]!["format"]!["type"]!.GetValue<string>().ShouldBe("json_schema");
        body["text"]!["format"]!["name"]!.GetValue<string>().ShouldBe(nameof(TestDocument));
        body["text"]!["format"]!["strict"]!.GetValue<bool>().ShouldBeTrue();
        body["reasoning"]!["effort"]!.GetValue<string>().ShouldBe("low");
    }

    [Fact]
    public async Task GenerateStructuredResponseAsync_WhenOutputIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            ResponseJson(
                "completed",
                """[{"type":"message","id":"msg_1","role":"assistant","status":"completed","content":[{"type":"output_text","text":"","annotations":[],"logprobs":[]}]}]""",
                null
            )
        );
        var client = CreateClient(handler);

        // Act
        var result = await client.GenerateStructuredResponseAsync<TestDocument>(
            "test-model",
            "Extract.",
            [new MessageDto("{}", RoleDto.User)]
        );

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task StreamImageGenerationAsync_WhenStreamCompletes_ShouldYieldTheImageSkipProgressAndSendAnUnsavedStreamedRequest()
    {
        // Arrange
        var content = Convert.ToBase64String([1, 2, 3]);
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                (
                    "image_generation.status",
                    """{"type":"image_generation.status","sequence_number":0,"stage":"queued","waiting_gens":1,"loading_models":0,"live_gens":0}"""
                ),
                (
                    "image_generation.progress",
                    """{"type":"image_generation.progress","sequence_number":1,"image_index":0,"percent":0.5,"step_percent":0.5,"total_percent":0.5,"preview_b64":null}"""
                ),
                (
                    "image_generation.completed",
                    $$"""{"type":"image_generation.completed","sequence_number":2,"image_index":0,"b64_json":"{{content}}","seed":42,"size":"832x1216","output_format":"jpeg"}"""
                ),
                (
                    "image_generation.done",
                    """{"type":"image_generation.done","sequence_number":3,"created":1700000000,"model":"flux","count":1}"""
                )
            )
        );

        var client = CreateImageClient(handler);

        // Act
        var events = await CollectAsync(
            client.StreamImageGenerationAsync(new GenerateImageRequest("A fox", 832, 1216, "flux"))
        );

        // Assert
        events.Count.ShouldBe(2);
        var completed = events[0].ShouldBeOfType<ImageGenerationCompletedDto>();
        completed.Content.ShouldBe(new byte[] { 1, 2, 3 });
        completed.MediaType.ShouldBe("image/jpeg");
        completed.Width.ShouldBe(832);
        completed.Height.ShouldBe(1216);
        completed.Seed.ShouldBe(42);
        events[1].ShouldBe(new ImageGenerationDoneDto("flux"));

        handler.RequestUri.ShouldBe(new Uri("http://test.com/v1/images/generations"));
        var body = handler.RequestBody.ShouldNotBeNull();
        body["prompt"]!.GetValue<string>().ShouldBe("A fox");
        body["model"]!.GetValue<string>().ShouldBe("flux");
        body["size"]!.GetValue<string>().ShouldBe("832x1216");
        body["n"]!.GetValue<int>().ShouldBe(1);
        body["stream"]!.GetValue<bool>().ShouldBeTrue();
        body["save"]!.GetValue<bool>().ShouldBeFalse();
        body["previews"]!.GetValue<bool>().ShouldBeFalse();
    }

    [Fact]
    public async Task StreamImageEditAsync_WhenStreamCompletes_ShouldYieldTheImageAndPostToEdits()
    {
        // Arrange
        var content = Convert.ToBase64String([4, 5, 6]);
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                (
                    "image_generation.completed",
                    $$"""{"type":"image_generation.completed","sequence_number":0,"image_index":0,"b64_json":"{{content}}","seed":7,"size":"832x1216","output_format":"png"}"""
                ),
                (
                    "image_generation.done",
                    """{"type":"image_generation.done","sequence_number":1,"created":1700000000,"model":"flux","count":1}"""
                )
            )
        );

        var client = CreateImageClient(handler);

        // Act
        var events = await CollectAsync(
            client.StreamImageEditAsync(new EditImageRequest("A fox at night", [1, 2, 3], 0.3f))
        );

        // Assert
        events.Count.ShouldBe(2);
        var completed = events[0].ShouldBeOfType<ImageGenerationCompletedDto>();
        completed.Content.ShouldBe(new byte[] { 4, 5, 6 });
        completed.Width.ShouldBe(832);
        events[1].ShouldBe(new ImageGenerationDoneDto("flux"));

        handler.RequestUri.ShouldBe(new Uri("http://test.com/v1/images/edits"));
        var body = handler.RequestBody.ShouldNotBeNull();
        body["image"]!.GetValue<string>().ShouldBe(Convert.ToBase64String([1, 2, 3]));
        body["strength"]!.GetValue<float>().ShouldBe(0.3f);
    }

    [Fact]
    public async Task StreamImageEditAsync_WhenStreamEndsWithoutDone_ShouldThrow()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                (
                    "image_generation.status",
                    """{"type":"image_generation.status","sequence_number":0,"stage":"queued","waiting_gens":0,"loading_models":0,"live_gens":0}"""
                )
            )
        );

        var client = CreateImageClient(handler);

        // Act
        var act = () =>
            CollectAsync(
                client.StreamImageEditAsync(new EditImageRequest("A fox", [1, 2, 3], 0.5f))
            );

        // Assert
        var exception = await act.ShouldThrowAsync<OuranosMlRequestException>();
        exception.Type.ShouldBe("incomplete_stream");
    }

    [Fact]
    public async Task StreamImageGenerationAsync_WhenRequestFails_ShouldThrowWithOuranosMlsError()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            """{"error":{"message":"Model 'x' not found.","type":"model_not_found"}}""",
            HttpStatusCode.NotFound
        );
        var client = CreateImageClient(handler);

        // Act
        var act = () =>
            CollectAsync(
                client.StreamImageGenerationAsync(new GenerateImageRequest("A fox", 64, 64))
            );

        // Assert
        var exception = await act.ShouldThrowAsync<OuranosMlRequestException>();
        exception.Type.ShouldBe("model_not_found");
        exception.Message.ShouldBe("Model 'x' not found.");
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StreamImageGenerationAsync_WhenErrorBodyIsNotOuranosMls_ShouldThrowWithTheStatus()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            """{"detail":[{"msg":"bad"}]}""",
            HttpStatusCode.UnprocessableEntity
        );
        var client = CreateImageClient(handler);

        // Act
        var act = () =>
            CollectAsync(
                client.StreamImageGenerationAsync(new GenerateImageRequest("A fox", 64, 64))
            );

        // Assert
        var exception = await act.ShouldThrowAsync<OuranosMlRequestException>();
        exception.Type.ShouldBe("http_error");
        exception.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task StreamImageGenerationAsync_WhenStreamReportsAnError_ShouldThrow()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                (
                    "error",
                    """{"type":"error","sequence_number":0,"code":"timeout","message":"Generation timed out."}"""
                )
            )
        );
        var client = CreateImageClient(handler);

        // Act
        var act = () =>
            CollectAsync(
                client.StreamImageGenerationAsync(new GenerateImageRequest("A fox", 64, 64))
            );

        // Assert
        var exception = await act.ShouldThrowAsync<OuranosMlRequestException>();
        exception.Type.ShouldBe("timeout");
        exception.Message.ShouldBe("Generation timed out.");
        exception.StatusCode.ShouldBeNull();
    }

    [Fact]
    public async Task StreamImageGenerationAsync_WhenStreamEndsWithoutDone_ShouldThrow()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "text/event-stream",
            Sse(
                (
                    "image_generation.status",
                    """{"type":"image_generation.status","sequence_number":0,"stage":"generating","waiting_gens":0,"loading_models":0,"live_gens":1}"""
                )
            )
        );
        var client = CreateImageClient(handler);

        // Act
        var act = () =>
            CollectAsync(
                client.StreamImageGenerationAsync(new GenerateImageRequest("A fox", 64, 64))
            );

        // Assert
        var exception = await act.ShouldThrowAsync<OuranosMlRequestException>();
        exception.Type.ShouldBe("incomplete_stream");
    }

    [Fact]
    public async Task GetImageLorasAsync_WhenListed_ShouldReturnTheLorasIgnoringUnknownFields()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            """{"object":"list","data":[{"id":"NSFW/detail","title":"Detail","architecture":"flux/lora","compat_class":"flux-1","default_weight":0.7,"trigger_phrase":"detailed","usage_hint":null}]}"""
        );
        var client = CreateImageClient(handler);

        // Act
        var loras = await client.GetImageLorasAsync();

        // Assert
        handler.RequestUri.ShouldBe(new Uri("http://test.com/v1/images/loras"));
        loras.ShouldBe([
            new ImageLoraDto("NSFW/detail", "Detail", "flux-1", 0.7f, "detailed", null),
        ]);
    }

    [Fact]
    public async Task GetImageModelsAsync_WhenListed_ShouldReturnTheModelsAndTheDefault()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            """{"object":"list","default_model":"flux","data":[{"id":"flux","title":"Flux","compat_class":"flux-1","default_size":"1024x1024","loaded":true}]}"""
        );
        var client = CreateImageClient(handler);

        // Act
        var models = await client.GetImageModelsAsync();

        // Assert
        handler.RequestUri.ShouldBe(new Uri("http://test.com/v1/images/models"));
        models.DefaultModel.ShouldBe("flux");
        models.Models.ShouldBe([new ImageModelDto("flux", "flux-1")]);
    }

    [Fact]
    public async Task GetImageLorasAsync_WhenRequestFails_ShouldThrow()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(
            "application/json",
            "{}",
            HttpStatusCode.BadGateway
        );
        var client = CreateImageClient(handler);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() => client.GetImageLorasAsync());
    }

    private OuranosMachineLearningClient CreateImageClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.com/v1/") };
        return new OuranosMachineLearningClient(_logger, httpClient, _openAiClient);
    }

    private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source)
    {
        var items = new List<T>();
        await foreach (var item in source)
        {
            items.Add(item);
        }

        return items;
    }

    private OuranosMachineLearningClient CreateClient(HttpMessageHandler handler)
    {
        var openAiClient = new OpenAIClient(
            new ApiKeyCredential("test"),
            new OpenAIClientOptions
            {
                Endpoint = new Uri("http://test.com/v1/"),
                Transport = new HttpClientPipelineTransport(new HttpClient(handler)),
            }
        );
        return new OuranosMachineLearningClient(_logger, _httpClient, openAiClient);
    }

    private static string ResponseJson(string status, string output, string? usage)
    {
        return $$$"""{"id":"resp_1","object":"response","created_at":1700000000,"status":"{{{status}}}","error":null,"incomplete_details":null,"model":"test-model","instructions":null,"output":{{{output}}},"usage":{{{usage ?? "null"}}},"tools":[],"tool_choice":"auto","parallel_tool_calls":true,"metadata":{}}""";
    }

    private static string Lifecycle(string type, int sequence, string status, string? usage)
    {
        var response = ResponseJson(status, "[]", usage);
        return $$$"""{"type":"{{{type}}}","sequence_number":{{{sequence}}},"response":{{{response}}}}""";
    }

    private static string Sse(params (string Event, string Data)[] events)
    {
        return string.Concat(events.Select(e => $"event: {e.Event}\ndata: {e.Data}\n\n"));
    }

    private sealed class RecordingHttpMessageHandler(
        string contentType,
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK
    ) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public JsonObject? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestUri = request.RequestUri;
            if (request.Content is not null)
            {
                RequestBody = JsonNode
                    .Parse(await request.Content.ReadAsStringAsync(cancellationToken))
                    ?.AsObject();
            }

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, contentType),
            };
        }
    }
}
