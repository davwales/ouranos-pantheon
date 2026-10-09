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
        _assistant = new TestAssistant(Substitute.For<ILogger>(), _mlClient);
    }

    public sealed record TestContext(string Topic);

    private sealed class TestAssistant(ILogger logger, IOuranosMachineLearningClient mlClient)
        : PantheonAssistant<TestContext>(logger, mlClient)
    {
        public TestContext? ReceivedContext { get; private set; }

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

    private void SetupStream(IAsyncEnumerable<ResponseStreamChunk> stream)
    {
        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(stream);
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
            new AssistantUsageEvent(10, 2, 12),
            new AssistantDoneEvent(),
        ]);
    }

    [Fact]
    public async Task Handle_WhenChunkHasNoTextOrReasoning_ShouldNotYieldEvents()
    {
        // Arrange
        SetupStream(
            CreateStream([
                new ResponseStreamChunk(string.Empty, string.Empty, null),
                new("Hi", null, null),
            ])
        );

        // Act
        var events = await CollectAsync(_assistant.Handle(CreateInput()));

        // Assert
        events.ShouldBe([new AssistantContentEvent("Hi"), new AssistantDoneEvent()]);
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
            new AssistantErrorEvent(PantheonAssistant<TestContext>.GenerationFailedMessage),
        ]);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldPassContextAndSendPromptAsInstructionsWithMappedMessages()
    {
        // Arrange
        List<MessageDto>? sentMessages = null;

        _mlClient
            .StreamResponseAsync(
                "test-model",
                "You are a test.",
                Arg.Do<List<MessageDto>>(m => sentMessages = m),
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
            new MessageDto("Hi", RoleDto.User),
            new MessageDto("Hello!", RoleDto.Assistant),
            new MessageDto("Help me", RoleDto.User),
        ]);
    }

    [Fact]
    public async Task Handle_WhenStreamFails_ShouldYieldErrorEventAndStop()
    {
        // Arrange
        SetupStream(
            CreateStream(
                [new ResponseStreamChunk("Partial", null, null)],
                new HttpRequestException("boom")
            )
        );

        // Act
        var events = await CollectAsync(_assistant.Handle(CreateInput()));

        // Assert
        events.ShouldBe([
            new AssistantContentEvent("Partial"),
            new AssistantErrorEvent(PantheonAssistant<TestContext>.GenerationFailedMessage),
        ]);
    }

    [Fact]
    public async Task Handle_WhenStreamIsCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        SetupStream(
            CreateStream(
                [new ResponseStreamChunk("Hi", null, null)],
                new OperationCanceledException()
            )
        );

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            CollectAsync(_assistant.Handle(CreateInput()))
        );
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
            new TestAssistant(Substitute.For<ILogger>(), null!)
        );
    }
}
