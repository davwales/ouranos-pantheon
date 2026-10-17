using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class PantheonAssistantTests
{
    private static readonly AssistantPrompt Prompt = new(
        "test-model",
        "You are a test.",
        0.5f,
        256,
        ReasoningEffort.Medium
    );

    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly TestAssistant _assistant;

    public PantheonAssistantTests()
    {
        _assistant = new TestAssistant(Substitute.For<ILogger>(), _mlClient, []);
    }

    public sealed record TestContext(string Topic);

    public sealed record LookupArguments(string Query);

    private sealed class LookupTool(bool isAvailable = true)
        : AssistantTool<TestContext, LookupArguments>(Substitute.For<ILogger>())
    {
        public override string Name => "lookup";

        public override string Description => "Looks something up.";

        public override bool IsAvailable(TestContext context)
        {
            return isAvailable;
        }

        protected override ValueTask<AssistantToolResult> ExecuteAsync(
            TestContext context,
            LookupArguments arguments,
            CancellationToken cancellationToken
        )
        {
            return ValueTask.FromResult(
                new AssistantToolResult($"{context.Topic}: {arguments.Query}")
            );
        }
    }

    private sealed class TestAssistant(
        ILogger logger,
        IOuranosMachineLearningClient mlClient,
        IEnumerable<IAssistantTool<TestContext>> tools
    ) : PantheonAssistant<TestContext>(logger, mlClient, tools)
    {
        public TestContext? ReceivedContext { get; private set; }

        public AssistantPrompt Prompt { get; init; } = PantheonAssistantTests.Prompt;

        protected override ValueTask<AssistantPrompt> BuildPromptAsync(
            TestContext context,
            CancellationToken cancellationToken
        )
        {
            ReceivedContext = context;
            return ValueTask.FromResult(Prompt);
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

    private static ResponseStreamChunk Call(string callId, string arguments, string name = "lookup")
    {
        return new ResponseStreamChunk(
            null,
            null,
            null,
            new FunctionCallDto(callId, name, arguments)
        );
    }

    private static AssistantCompletionInput<TestContext> CreateInput(TestContext? context = null)
    {
        return new(
            [
                new AssistantMessageInput("Hi", AssistantRole.User),
                new AssistantMessageInput("Hello!", AssistantRole.Assistant),
                new AssistantMessageInput("Help me", AssistantRole.User),
            ],
            context ?? new TestContext("baking")
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
    public async Task Handle_WhenHappyPath_ShouldStreamReasoningThenContentThenUsageThenDone()
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
        var events = await CollectAsync(_assistant.Handle(CreateInput()));

        // Assert
        events.ShouldBe([
            new AssistantReasoningEvent("Thinking"),
            new AssistantContentEvent("Hello"),
            new AssistantContentEvent(" world"),
            new AssistantUsageEvent(10, 2, 12, 12),
            new AssistantDoneEvent(),
        ]);
    }

    [Fact]
    public async Task Handle_WhenStreamEndsWithoutContent_ShouldYieldErrorEventInsteadOfDone()
    {
        // Arrange
        SetupStream(
            CreateStream([
                new ResponseStreamChunk(null, "Thinking", null),
                new ResponseStreamChunk(null, null, new ChatCompletionUsage(10, 256, 266)),
            ])
        );

        // Act
        var events = await CollectAsync(_assistant.Handle(CreateInput()));

        // Assert
        events.ShouldBe([
            new AssistantReasoningEvent("Thinking"),
            new AssistantErrorEvent(AssistantToolLoop<TestContext>.GenerationFailedMessage),
        ]);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldSendPromptWithFormattingGuidelineAsInstructionsWithMappedMessages()
    {
        // Arrange
        IReadOnlyList<ResponseInputItemDto>? sentMessages = null;

        _mlClient
            .StreamResponseAsync(
                "test-model",
                $"You are a test.\n\n{AssistantFormatting.Instructions}",
                Arg.Do<IReadOnlyList<ResponseInputItemDto>>(m => sentMessages = m),
                Arg.Is<IReadOnlyList<FunctionToolDto>>(t => t.Count == 0),
                0.5f,
                256,
                ReasoningEffort.Medium,
                Arg.Any<CancellationToken>()
            )
            .Returns(CreateStream([new ResponseStreamChunk("Hi", null, null)]));

        var input = CreateInput(new TestContext("grilling"));

        // Act
        await CollectAsync(_assistant.Handle(input));

        // Assert
        _assistant.ReceivedContext.ShouldBe(new TestContext("grilling"));
        sentMessages.ShouldNotBeNull();
        sentMessages.ShouldBe([
            Message("Hi", RoleDto.User),
            Message("Hello!", RoleDto.Assistant),
            Message("Help me", RoleDto.User),
        ]);
    }

    [Fact]
    public async Task Handle_WhenCancelledBeforeStart_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            CollectAsync(_assistant.Handle(CreateInput(), cancellationToken))
        );
    }

    [Fact]
    public async Task Handle_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var input = new AssistantCompletionInput<TestContext>([], null!);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            CollectAsync(_assistant.Handle(input))
        );
    }

    [Fact]
    public void Constructor_WhenMlClientIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TestAssistant(Substitute.For<ILogger>(), null!, [])
        );
    }

    [Fact]
    public void Constructor_WhenToolsAreNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TestAssistant(Substitute.For<ILogger>(), _mlClient, null!)
        );
    }

    private static MessageInputItemDto Message(string content, RoleDto role)
    {
        return new MessageInputItemDto(new MessageDto(content, role));
    }

    private async Task<IReadOnlyList<ResponseInputItemDto>> SendWithBudgetAsync(
        int historyCharacterBudget,
        AssistantMessageInput[] messages
    )
    {
        IReadOnlyList<ResponseInputItemDto>? sentMessages = null;

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Do<IReadOnlyList<ResponseInputItemDto>>(m => sentMessages = m),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(CreateStream([new ResponseStreamChunk("Ok", null, null)]));

        var assistant = new TestAssistant(Substitute.For<ILogger>(), _mlClient, [])
        {
            Prompt = Prompt with { HistoryCharacterBudget = historyCharacterBudget },
        };

        await CollectAsync(assistant.Handle(new([.. messages], new TestContext("baking"))));

        sentMessages.ShouldNotBeNull();
        return sentMessages;
    }

    [Fact]
    public async Task Handle_WhenHistoryFitsTheBudget_ShouldSendEveryMessage()
    {
        // Arrange
        AssistantMessageInput[] messages =
        [
            new AssistantMessageInput("Hi", AssistantRole.User),
            new AssistantMessageInput("Hello!", AssistantRole.Assistant),
            new AssistantMessageInput("Help me", AssistantRole.User),
        ];

        // Act
        var sent = await SendWithBudgetAsync(100, messages);

        // Assert
        sent.ShouldBe([
            Message("Hi", RoleDto.User),
            Message("Hello!", RoleDto.Assistant),
            Message("Help me", RoleDto.User),
        ]);
    }

    [Fact]
    public async Task Handle_WhenHistoryExceedsTheBudget_ShouldSendOnlyTheNewestTurnsThatFit()
    {
        // Arrange
        AssistantMessageInput[] messages =
        [
            new AssistantMessageInput("First question", AssistantRole.User),
            new AssistantMessageInput("First answer", AssistantRole.Assistant),
            new AssistantMessageInput("Second question", AssistantRole.User),
            new AssistantMessageInput("Answer 2", AssistantRole.Assistant),
            new AssistantMessageInput("Third", AssistantRole.User),
        ];

        // Act
        var sent = await SendWithBudgetAsync(30, messages);

        // Assert
        sent.ShouldBe([
            Message("Second question", RoleDto.User),
            Message("Answer 2", RoleDto.Assistant),
            Message("Third", RoleDto.User),
        ]);
    }

    [Fact]
    public async Task Handle_WhenTrimmedHistoryWouldStartWithAnAnswer_ShouldDropTheAnswer()
    {
        // Arrange
        AssistantMessageInput[] messages =
        [
            new AssistantMessageInput("First question", AssistantRole.User),
            new AssistantMessageInput("First answer", AssistantRole.Assistant),
            new AssistantMessageInput("Second", AssistantRole.User),
        ];

        // Act
        var sent = await SendWithBudgetAsync(20, messages);

        // Assert
        sent.ShouldBe([Message("Second", RoleDto.User)]);
    }

    [Fact]
    public async Task Handle_WhenNewestMessageAloneExceedsTheBudget_ShouldStillSendIt()
    {
        // Arrange
        AssistantMessageInput[] messages =
        [
            new AssistantMessageInput("Hi", AssistantRole.User),
            new AssistantMessageInput("Hello!", AssistantRole.Assistant),
            new AssistantMessageInput("A long question", AssistantRole.User),
        ];

        // Act
        var sent = await SendWithBudgetAsync(5, messages);

        // Assert
        sent.ShouldBe([Message("A long question", RoleDto.User)]);
    }

    [Fact]
    public async Task Handle_WhenModelCallsATool_ShouldSendTheOutputBackAndStreamTheAnswer()
    {
        // Arrange
        var sentInputs = new List<IReadOnlyList<ResponseInputItemDto>>();
        var sentTools = new List<IReadOnlyList<FunctionToolDto>>();
        string? sentInstructions = null;

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Do<string>(i => sentInstructions = i),
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

        var assistant = new TestAssistant(Substitute.For<ILogger>(), _mlClient, [new LookupTool()]);

        // Act
        var events = await CollectAsync(assistant.Handle(CreateInput()));

        // Assert
        events.ShouldBe([
            new AssistantReasoningEvent("Need data"),
            new AssistantToolEvent("call-1", "lookup", """{"query":"flour"}"""),
            new AssistantToolResultEvent(
                "call-1",
                AssistantToolStatus.Completed,
                "baking: flour",
                false
            ),
            new AssistantContentEvent("Use bread flour."),
            new AssistantUsageEvent(30, 8, 38, 23),
            new AssistantDoneEvent(),
        ]);

        sentInstructions.ShouldNotBeNull();
        sentInstructions.ShouldEndWith(AssistantFormatting.ToolInstructions);
        sentTools.Count.ShouldBe(2);
        sentTools[0].Single().Name.ShouldBe("lookup");
        sentTools[0].Single().Description.ShouldBe("Looks something up.");

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
    public async Task Handle_WhenAnAnswerCarriesActionOutcomes_ShouldAppendThemAsANoteForTheModel()
    {
        // Arrange
        AssistantMessageInput[] messages =
        [
            new AssistantMessageInput("Add salt", AssistantRole.User),
            new AssistantMessageInput(
                "Proposed.",
                AssistantRole.Assistant,
                [
                    new AssistantActionOutcome(
                        "add_ingredient",
                        "Add 1 tsp salt",
                        AssistantActionStatus.Rejected
                    ),
                    new AssistantActionOutcome("set_view", " ", AssistantActionStatus.Applied),
                    new AssistantActionOutcome("add_note", "Note", AssistantActionStatus.Pending),
                    new AssistantActionOutcome("add_step", "Step", AssistantActionStatus.Failed),
                ]
            ),
            new AssistantMessageInput("Why?", AssistantRole.User),
        ];

        // Act
        var sent = await SendWithBudgetAsync(1000, messages);

        // Assert
        sent.ShouldBe([
            Message("Add salt", RoleDto.User),
            Message(
                "Proposed.\n\n[Proposals: \"Add 1 tsp salt\" (add_ingredient): rejected by the user; "
                    + "set_view: applied by the user; \"Note\" (add_note): not acted on yet; "
                    + "\"Step\" (add_step): failed to apply]",
                RoleDto.Assistant
            ),
            Message("Why?", RoleDto.User),
        ]);
    }

    [Fact]
    public async Task Handle_WhenActionOutcomesExceedTheBudget_ShouldCountThemWhenTrimming()
    {
        // Arrange
        AssistantMessageInput[] messages =
        [
            new AssistantMessageInput("Add salt", AssistantRole.User),
            new AssistantMessageInput(
                "Ok.",
                AssistantRole.Assistant,
                [
                    new AssistantActionOutcome(
                        "add_ingredient",
                        "Add 1 tsp salt",
                        AssistantActionStatus.Applied
                    ),
                ]
            ),
            new AssistantMessageInput("Thanks", AssistantRole.User),
        ];

        // Act
        var sent = await SendWithBudgetAsync(30, messages);

        // Assert
        sent.ShouldBe([Message("Thanks", RoleDto.User)]);
    }

    [Fact]
    public async Task Handle_WhenToolIsUnavailableForTheContext_ShouldNotOfferIt()
    {
        // Arrange
        string? sentInstructions = null;
        IReadOnlyList<FunctionToolDto>? sentTools = null;

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Do<string>(i => sentInstructions = i),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Do<IReadOnlyList<FunctionToolDto>>(t => sentTools = t),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(CreateStream([new ResponseStreamChunk("Hi", null, null)]));

        var assistant = new TestAssistant(
            Substitute.For<ILogger>(),
            _mlClient,
            [new LookupTool(isAvailable: false)]
        );

        // Act
        await CollectAsync(assistant.Handle(CreateInput()));

        // Assert
        sentTools.ShouldNotBeNull();
        sentTools.ShouldBeEmpty();
        sentInstructions.ShouldNotBeNull();
        sentInstructions.ShouldNotContain(AssistantFormatting.ToolInstructions);
    }
}
