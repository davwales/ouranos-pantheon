using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OpenAI;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
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
                [new MessageDto("Hi", RoleDto.User), new MessageDto("Hello", RoleDto.Assistant)],
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
                [new MessageDto("Hi", RoleDto.User)]
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
                    [new MessageDto("Hi", RoleDto.User)]
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
                    [new MessageDto("Hi", RoleDto.User)]
                )
            ) { }
        };

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
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

    private sealed class RecordingHttpMessageHandler(string contentType, string content)
        : HttpMessageHandler
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, contentType),
            };
        }
    }
}
