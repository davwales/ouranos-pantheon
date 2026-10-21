using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class AssistantToolLoopTests
{
    private const string Instructions = "You are a test.";

    private static readonly AssistantPrompt Prompt = new(
        "test-model",
        "You are a test.",
        0.5f,
        256,
        ReasoningEffort.Medium
    );

    private static readonly TestContext Context = new("baking");

    private static readonly IReadOnlyList<ResponseInputItemDto> History =
    [
        new MessageInputItemDto(new MessageDto("Hi", RoleDto.User)),
        new MessageInputItemDto(new MessageDto("Hello!", RoleDto.Assistant)),
        new MessageInputItemDto(new MessageDto("Help me", RoleDto.User)),
    ];

    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly AssistantToolLoop<TestContext> _loop;

    public AssistantToolLoopTests()
    {
        _timeProvider.TimestampFrequency.Returns(1000);
        _loop = new AssistantToolLoop<TestContext>(
            Substitute.For<ILogger>(),
            _mlClient,
            _timeProvider
        );
    }

    public sealed record TestContext(string Topic);

    public sealed record LookupArguments(string Query);

    public sealed record TestAction(string Query);

    private sealed class LookupTool(
        Func<TestContext, LookupArguments, AssistantToolResult>? execute = null,
        bool isSingleDeliveryPerReply = false
    ) : AssistantTool<TestContext, LookupArguments>(Substitute.For<ILogger>())
    {
        public override string Name => "lookup";

        public override string Description => "Looks something up.";

        public override bool IsSingleDeliveryPerReply => isSingleDeliveryPerReply;

        protected override ValueTask<AssistantToolResult> ExecuteAsync(
            TestContext context,
            LookupArguments arguments,
            CancellationToken cancellationToken
        )
        {
            return ValueTask.FromResult(
                execute?.Invoke(context, arguments)
                    ?? new AssistantToolResult($"{context.Topic}: {arguments.Query}")
            );
        }
    }

    private static async IAsyncEnumerable<ResponseStreamChunk> CreateStream(
        IEnumerable<ResponseStreamChunk> chunks,
        Exception? failAfter = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return chunk;
        }

        if (failAfter is not null)
        {
            throw failAfter;
        }
    }

    private void SetupStream(
        IAsyncEnumerable<ResponseStreamChunk> stream,
        params IAsyncEnumerable<ResponseStreamChunk>[] laterRounds
    )
    {
        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(stream, laterRounds);
    }

    private List<IReadOnlyList<ResponseInputItemDto>> CaptureInputs(
        IAsyncEnumerable<ResponseStreamChunk> stream,
        params IAsyncEnumerable<ResponseStreamChunk>[] laterRounds
    )
    {
        var sentInputs = new List<IReadOnlyList<ResponseInputItemDto>>();

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Do<IReadOnlyList<ResponseInputItemDto>>(sentInputs.Add),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(stream, laterRounds);

        return sentInputs;
    }

    private static ResponseStreamChunk Call(string callId, string arguments, string name = "lookup")
    {
        return new ResponseStreamChunk(
            null,
            null,
            null,
            new FunctionCallDto(callId, name, arguments)
        );
    }

    private Task<List<AssistantEvent>> RunAsync(
        IReadOnlyList<IAssistantTool<TestContext>> tools,
        AssistantPrompt? prompt = null,
        CancellationToken cancellationToken = default
    )
    {
        return CollectAsync(
            _loop.RunAsync(
                prompt ?? Prompt,
                Instructions,
                History,
                tools,
                Context,
                cancellationToken
            )
        );
    }

    private static async Task<List<AssistantEvent>> CollectAsync(
        IAsyncEnumerable<AssistantEvent> stream
    )
    {
        var events = new List<AssistantEvent>();
        await foreach (var assistantEvent in stream)
        {
            events.Add(assistantEvent);
        }

        return events;
    }

    [Fact]
    public async Task RunAsync_WhenHappyPath_ShouldStreamReasoningThenContentThenUsageWithoutDone()
    {
        // Arrange
        SetupStream(
            CreateStream([
                new ResponseStreamChunk(null, "Thinking", null),
                new ResponseStreamChunk("Hello", null, null),
                new ResponseStreamChunk(" world", null, null),
                new ResponseStreamChunk(null, null, new ChatCompletionUsage(10, 2, 12)),
            ])
        );

        // Act
        var events = await RunAsync([]);

        // Assert
        events.ShouldBe([
            new AssistantReasoningEvent("Thinking"),
            new AssistantContentEvent("Hello"),
            new AssistantContentEvent(" world"),
            new AssistantUsageEvent(10, 2, 12, 12),
        ]);
    }

    [Fact]
    public async Task RunAsync_WhenCalled_ShouldSendTheInstructionsAndHistoryVerbatim()
    {
        // Arrange
        var sentInputs = new List<IReadOnlyList<ResponseInputItemDto>>();

        _mlClient
            .StreamResponseAsync(
                "test-model",
                Instructions,
                Arg.Do<IReadOnlyList<ResponseInputItemDto>>(sentInputs.Add),
                Arg.Is<IReadOnlyList<FunctionToolDto>>(t => t.Count == 0),
                0.5f,
                256,
                ReasoningEffort.Medium,
                Arg.Any<CancellationToken>()
            )
            .Returns(CreateStream([new ResponseStreamChunk("Hi", null, null)]));

        // Act
        var events = await RunAsync([]);

        // Assert
        events.ShouldBe([new AssistantContentEvent("Hi")]);
        sentInputs.Single().ShouldBe(History);
    }

    [Fact]
    public async Task RunAsync_WhenChunkHasNoTextOrReasoning_ShouldNotYieldEvents()
    {
        // Arrange
        SetupStream(
            CreateStream([
                new ResponseStreamChunk(string.Empty, string.Empty, null),
                new("Hi", null, null),
            ])
        );

        // Act
        var events = await RunAsync([]);

        // Assert
        events.ShouldBe([new AssistantContentEvent("Hi")]);
    }

    [Fact]
    public async Task RunAsync_WhenStreamEndsWithoutContent_ShouldYieldErrorEvent()
    {
        // Arrange
        SetupStream(
            CreateStream([
                new ResponseStreamChunk(null, "Thinking", null),
                new ResponseStreamChunk(null, null, new ChatCompletionUsage(10, 256, 266)),
            ])
        );

        // Act
        var events = await RunAsync([]);

        // Assert
        events.ShouldBe([
            new AssistantReasoningEvent("Thinking"),
            new AssistantErrorEvent(AssistantToolLoop<TestContext>.GenerationFailedMessage),
        ]);
    }

    [Fact]
    public async Task RunAsync_WhenStreamFails_ShouldYieldErrorEventAndStop()
    {
        // Arrange
        SetupStream(
            CreateStream(
                [new ResponseStreamChunk("Partial", null, null)],
                new HttpRequestException("boom")
            )
        );

        // Act
        var events = await RunAsync([]);

        // Assert
        events.ShouldBe([
            new AssistantContentEvent("Partial"),
            new AssistantErrorEvent(AssistantToolLoop<TestContext>.GenerationFailedMessage),
        ]);
    }

    [Fact]
    public async Task RunAsync_WhenStreamIsCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        SetupStream(CreateStream([new ResponseStreamChunk("Hi", null, null)]));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            RunAsync([], cancellationToken: new CancellationToken(canceled: true))
        );
    }

    [Fact]
    public async Task RunAsync_WhenStreamTimesOut_ShouldYieldErrorEventAndStop()
    {
        // Arrange
        SetupStream(
            CreateStream(
                [new ResponseStreamChunk("Partial", null, null)],
                new TaskCanceledException("The request timed out.")
            )
        );

        // Act
        var events = await RunAsync([]);

        // Assert
        events.ShouldBe([
            new AssistantContentEvent("Partial"),
            new AssistantErrorEvent(AssistantToolLoop<TestContext>.GenerationFailedMessage),
        ]);
    }

    [Fact]
    public async Task RunAsync_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            CollectAsync(_loop.RunAsync(Prompt, Instructions, History, [], null!))
        );
    }

    [Fact]
    public async Task RunAsync_WhenInstructionsAreBlank_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() =>
            CollectAsync(_loop.RunAsync(Prompt, " ", History, [], Context))
        );
    }

    [Fact]
    public void Constructor_WhenMlClientIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new AssistantToolLoop<TestContext>(Substitute.For<ILogger>(), null!)
        );
    }

    [Fact]
    public async Task RunAsync_WhenModelCallsATool_ShouldSendTheOutputBackAndStreamTheAnswer()
    {
        // Arrange
        var sentInputs = new List<IReadOnlyList<ResponseInputItemDto>>();
        var sentTools = new List<IReadOnlyList<FunctionToolDto>>();

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Instructions,
                Arg.Do<IReadOnlyList<ResponseInputItemDto>>(sentInputs.Add),
                Arg.Do<IReadOnlyList<FunctionToolDto>>(sentTools.Add),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                CreateStream([
                    new ResponseStreamChunk(null, "Need data", null),
                    Call("call-1", """{"query":"flour"}"""),
                    new ResponseStreamChunk(null, null, new ChatCompletionUsage(10, 5, 15)),
                ]),
                CreateStream([
                    new ResponseStreamChunk("Use bread flour.", null, null),
                    new ResponseStreamChunk(null, null, new ChatCompletionUsage(20, 3, 23)),
                ])
            );

        // Act
        var events = await RunAsync([new LookupTool()]);

        // Assert
        events.ShouldBe([
            new AssistantReasoningEvent("Need data"),
            new AssistantToolEvent("call-1", "lookup", """{"query":"flour"}"""),
            new AssistantToolResultEvent(
                "call-1",
                AssistantToolStatus.Completed,
                "baking: flour",
                false,
                0
            ),
            new AssistantContentEvent("Use bread flour."),
            new AssistantUsageEvent(30, 8, 38, 23),
        ]);

        sentTools.Count.ShouldBe(2);
        sentTools[0].Single().Name.ShouldBe("lookup");
        sentTools[0].Single().Description.ShouldBe("Looks something up.");

        sentInputs[0].ShouldBe(History);
        sentInputs[1]
            .Skip(3)
            .ShouldBe([
                new FunctionCallInputItemDto(
                    new FunctionCallDto("call-1", "lookup", """{"query":"flour"}""")
                ),
                new FunctionCallOutputInputItemDto("call-1", "baking: flour"),
            ]);
    }

    [Fact]
    public async Task RunAsync_WhenToolReturnsAnAction_ShouldStreamTheActionBetweenTheCallAndItsResult()
    {
        // Arrange
        SetupStream(
            CreateStream([Call("call-1", """{"query":"salt"}""")]),
            CreateStream([new ResponseStreamChunk("Proposed.", null, null)])
        );

        // Act
        var events = await RunAsync([
            new LookupTool(
                (_, arguments) =>
                    new AssistantToolResult("Proposed", new TestAction(arguments.Query))
            ),
        ]);

        // Assert
        events.ShouldBe([
            new AssistantToolEvent("call-1", "lookup", """{"query":"salt"}"""),
            new AssistantActionEvent("call-1", "lookup", new TestAction("salt")),
            new AssistantToolResultEvent(
                "call-1",
                AssistantToolStatus.Completed,
                "Proposed",
                false,
                0
            ),
            new AssistantContentEvent("Proposed."),
        ]);
    }

    [Theory]
    [InlineData("unknown", """{"query":"x"}""", "Unknown tool 'unknown'.")]
    [InlineData(
        "lookup",
        "not json",
        AssistantTool<TestContext, LookupArguments>.InvalidArgumentsOutput
    )]
    public async Task RunAsync_WhenToolCallCannotBeServed_ShouldReportAFailureAndContinue(
        string name,
        string arguments,
        string expectedOutput
    )
    {
        // Arrange
        var sentInputs = CaptureInputs(
            CreateStream([Call("call-1", arguments, name)]),
            CreateStream([new ResponseStreamChunk("Sorry.", null, null)])
        );

        // Act
        var events = await RunAsync([new LookupTool()]);

        // Assert
        events.ShouldBe([
            new AssistantToolEvent("call-1", name, arguments),
            new AssistantToolResultEvent(
                "call-1",
                AssistantToolStatus.Failed,
                expectedOutput,
                false,
                0
            ),
            new AssistantContentEvent("Sorry."),
        ]);

        sentInputs[1].Last().ShouldBe(new FunctionCallOutputInputItemDto("call-1", expectedOutput));
    }

    [Fact]
    public async Task RunAsync_WhenToolThrows_ShouldTellTheModelTheToolFailed()
    {
        // Arrange
        var sentInputs = CaptureInputs(
            CreateStream([Call("call-1", """{"query":"x"}""")]),
            CreateStream([new ResponseStreamChunk("Sorry.", null, null)])
        );

        // Act
        var events = await RunAsync([
            new LookupTool((_, _) => throw new InvalidOperationException("boom")),
        ]);

        // Assert
        events.ShouldContain(
            new AssistantToolResultEvent(
                "call-1",
                AssistantToolStatus.Failed,
                AssistantToolLoop<TestContext>.ToolFailedOutput,
                false,
                0
            )
        );
        events.ShouldContain(new AssistantContentEvent("Sorry."));
        sentInputs[1]
            .Last()
            .ShouldBe(
                new FunctionCallOutputInputItemDto(
                    "call-1",
                    AssistantToolLoop<TestContext>.ToolFailedOutput
                )
            );
    }

    [Fact]
    public async Task RunAsync_WhenToolTimesOut_ShouldTellTheModelTheToolFailed()
    {
        // Arrange
        SetupStream(
            CreateStream([Call("call-1", """{"query":"x"}""")]),
            CreateStream([new ResponseStreamChunk("Sorry.", null, null)])
        );

        // Act
        var events = await RunAsync([
            new LookupTool((_, _) => throw new TaskCanceledException("The request timed out.")),
        ]);

        // Assert
        events.ShouldContain(
            new AssistantToolResultEvent(
                "call-1",
                AssistantToolStatus.Failed,
                AssistantToolLoop<TestContext>.ToolFailedOutput,
                false,
                0
            )
        );
        events.ShouldContain(new AssistantContentEvent("Sorry."));
    }

    [Fact]
    public async Task RunAsync_WhenToolIsCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        SetupStream(CreateStream([Call("call-1", """{"query":"x"}""")]));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            RunAsync(
                [
                    new LookupTool(
                        (_, _) =>
                        {
                            cts.Cancel();
                            throw new OperationCanceledException(cts.Token);
                        }
                    ),
                ],
                cancellationToken: cts.Token
            )
        );
    }

    [Fact]
    public async Task RunAsync_WhenToolOutputIsTooLong_ShouldTruncateItForTheModelAndTheClient()
    {
        // Arrange
        var sentInputs = CaptureInputs(
            CreateStream([Call("call-1", """{"query":"x"}""")]),
            CreateStream([new ResponseStreamChunk("Ok.", null, null)])
        );

        // Act
        var events = await RunAsync(
            [new LookupTool((_, _) => new AssistantToolResult(new string('a', 20)))],
            Prompt with
            {
                MaxToolOutputCharacters = 5,
            }
        );

        // Assert
        sentInputs[1].Last().ShouldBe(new FunctionCallOutputInputItemDto("call-1", "aaaaa…"));
        events.ShouldContain(
            new AssistantToolResultEvent("call-1", AssistantToolStatus.Completed, "aaaaa…", true, 0)
        );
    }

    [Fact]
    public async Task RunAsync_WhenToolRoundsAreExhausted_ShouldOfferNoToolsInTheLastRound()
    {
        // Arrange
        var sentTools = new List<IReadOnlyList<FunctionToolDto>>();

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Do<IReadOnlyList<FunctionToolDto>>(sentTools.Add),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                CreateStream([Call("call-1", """{"query":"a"}""")]),
                CreateStream([Call("call-2", """{"query":"b"}""")]),
                CreateStream([Call("call-3", """{"query":"c"}""")])
            );

        // Act
        var events = await RunAsync([new LookupTool()], Prompt with { MaxToolRounds = 2 });

        // Assert
        sentTools.Select(t => t.Count).ShouldBe([1, 1, 0]);
        events.Count(e => e is AssistantToolEvent).ShouldBe(2);

        events
            .Last()
            .ShouldBe(
                new AssistantErrorEvent(AssistantToolLoop<TestContext>.GenerationFailedMessage)
            );
    }

    [Fact]
    public async Task RunAsync_WhenARoundHasTooManyCalls_ShouldRunOnlyTheFirstAndReportTheRestAsSkipped()
    {
        // Arrange
        var executed = new List<string>();
        var sentInputs = CaptureInputs(
            CreateStream([
                Call("call-1", """{"query":"a"}"""),
                Call("call-2", """{"query":"b"}"""),
                Call("call-3", """{"query":"c"}"""),
            ]),
            CreateStream([new ResponseStreamChunk("Done.", null, null)])
        );

        // Act
        var events = await RunAsync(
            [
                new LookupTool(
                    (_, arguments) =>
                    {
                        executed.Add(arguments.Query);
                        return new AssistantToolResult(arguments.Query);
                    }
                ),
            ],
            Prompt with
            {
                MaxToolCallsPerRound = 2,
            }
        );

        // Assert
        executed.ShouldBe(["a", "b"]);
        events.OfType<AssistantToolEvent>().Count().ShouldBe(3);
        events
            .OfType<AssistantToolResultEvent>()
            .Select(result => result.Status)
            .ShouldBe([
                AssistantToolStatus.Completed,
                AssistantToolStatus.Completed,
                AssistantToolStatus.Skipped,
            ]);
        sentInputs[1]
            .OfType<FunctionCallOutputInputItemDto>()
            .Select(output => output.Output)
            .ShouldBe(["a", "b", AssistantToolLoop<TestContext>.TooManyToolCallsOutput]);
    }

    [Fact]
    public async Task RunAsync_WhenASingleDeliveryToolRepeatsInAReply_ShouldStreamOnlyTheFirstAction()
    {
        // Arrange
        var sentInputs = CaptureInputs(
            CreateStream([
                Call("call-1", """{"query":"bad"}"""),
                Call("call-2", """{"query":"first"}"""),
            ]),
            CreateStream([Call("call-3", """{"query":"second"}""")]),
            CreateStream([new ResponseStreamChunk("Proposed.", null, null)])
        );

        // Act
        var events = await RunAsync([
            new LookupTool(
                (_, arguments) =>
                    arguments.Query == "bad"
                        ? new AssistantToolResult("Invalid.")
                        : new AssistantToolResult("Proposed", new TestAction(arguments.Query)),
                isSingleDeliveryPerReply: true
            ),
        ]);

        // Assert
        events
            .OfType<AssistantActionEvent>()
            .ShouldBe([new AssistantActionEvent("call-2", "lookup", new TestAction("first"))]);
        events
            .OfType<AssistantToolResultEvent>()
            .Last()
            .ShouldBe(
                new AssistantToolResultEvent(
                    "call-3",
                    AssistantToolStatus.Skipped,
                    AssistantToolLoop<TestContext>.RepeatedCallOutput,
                    false,
                    0
                )
            );

        sentInputs[2]
            .OfType<FunctionCallOutputInputItemDto>()
            .Select(output => output.Output)
            .ShouldBe(["Invalid.", "Proposed", AssistantToolLoop<TestContext>.RepeatedCallOutput]);
    }

    [Fact]
    public async Task RunAsync_WhenAToolReturnsAnImage_ShouldStreamItAfterTheResult()
    {
        // Arrange
        var image = new AssistantImage("img-1", "/images/img-1", 64, 32, "A fox");

        CaptureInputs(
            CreateStream([Call("call-1", """{"query":"fox"}""")]),
            CreateStream([new ResponseStreamChunk("Here it is.", null, null)])
        );

        // Act
        var events = await RunAsync([
            new LookupTool((_, _) => new AssistantToolResult("Generated.", Image: image)),
        ]);

        // Assert
        var resultIndex = events.FindIndex(e => e is AssistantToolResultEvent);
        events[resultIndex + 1].ShouldBe(new AssistantImageEvent("call-1", image));
    }

    [Fact]
    public async Task RunAsync_WhenAnImageIsTheWholeReply_ShouldFinishWithoutAnError()
    {
        // Arrange
        var image = new AssistantImage("img-1", "/images/img-1", 64, 32, "A fox");

        CaptureInputs(
            CreateStream([Call("call-1", """{"query":"fox"}""")]),
            CreateStream([new ResponseStreamChunk(null, null, new ChatCompletionUsage(20, 1, 21))])
        );

        // Act
        var events = await RunAsync([
            new LookupTool((_, _) => new AssistantToolResult("Shown.", Image: image)),
        ]);

        // Assert
        events.OfType<AssistantErrorEvent>().ShouldBeEmpty();
        events.Last().ShouldBe(new AssistantUsageEvent(20, 1, 21, 21));
    }

    [Fact]
    public async Task RunAsync_WhenAToolRuns_ShouldReportHowLongItTook()
    {
        // Arrange
        _timeProvider.GetTimestamp().Returns(1_000, 3_500);

        CaptureInputs(
            CreateStream([Call("call-1", """{"query":"fox"}""")]),
            CreateStream([new ResponseStreamChunk("Done.", null, null)])
        );

        // Act
        var events = await RunAsync([new LookupTool()]);

        // Assert
        events.OfType<AssistantToolResultEvent>().Single().DurationMs.ShouldBe(2_500);
    }

    [Fact]
    public async Task RunAsync_WhenAFailedResultCarriesAnImage_ShouldNotStreamIt()
    {
        // Arrange
        CaptureInputs(
            CreateStream([Call("call-1", """{"query":"fox"}""")]),
            CreateStream([new ResponseStreamChunk("Sorry.", null, null)])
        );

        // Act
        var events = await RunAsync([
            new LookupTool(
                (_, _) =>
                    new AssistantToolResult(
                        "Failed.",
                        IsError: true,
                        Image: new AssistantImage("img-1", "/images/img-1", 64, 32, "A fox")
                    )
            ),
        ]);

        // Assert
        events.OfType<AssistantImageEvent>().ShouldBeEmpty();
    }

    [Fact]
    public async Task RunAsync_WhenASingleDeliveryToolDeliversAnImage_ShouldSkipLaterCalls()
    {
        // Arrange
        var executed = new List<string>();

        CaptureInputs(
            CreateStream([
                Call("call-1", """{"query":"first"}"""),
                Call("call-2", """{"query":"second"}"""),
            ]),
            CreateStream([new ResponseStreamChunk("Done.", null, null)])
        );

        // Act
        var events = await RunAsync([
            new LookupTool(
                (_, arguments) =>
                {
                    executed.Add(arguments.Query);
                    return new AssistantToolResult(
                        "Generated.",
                        Image: new AssistantImage(arguments.Query, "/x", 1, 1, "x")
                    );
                },
                isSingleDeliveryPerReply: true
            ),
        ]);

        // Assert
        executed.ShouldBe(["first"]);
        events.OfType<AssistantImageEvent>().Count().ShouldBe(1);

        events
            .OfType<AssistantToolResultEvent>()
            .Last()
            .Status.ShouldBe(AssistantToolStatus.Skipped);
    }
}
