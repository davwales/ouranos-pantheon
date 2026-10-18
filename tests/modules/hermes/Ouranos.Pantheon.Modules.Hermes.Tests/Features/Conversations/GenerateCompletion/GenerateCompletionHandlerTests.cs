using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandlerTests
{
    private readonly ILogger<GenerateCompletionHandler> _logger = Substitute.For<
        ILogger<GenerateCompletionHandler>
    >();
    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();

    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory = Substitute.For<
        IDbContextFactory<HermesDbContext>
    >();

    private readonly string _dbName = Guid.NewGuid().ToString();

    private string? _sentInstructions;
    private IReadOnlyList<FunctionToolDto>? _sentTools;

    private readonly GenerateCompletionHandler _handler;

    public GenerateCompletionHandlerTests()
    {
        _dbContextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(DbContextExtensions.Mock<HermesDbContext>(_dbName)));

        _handler = CreateHandler([]);
    }

    public sealed record EchoArguments(string Text);

    private sealed class EchoTool(bool isAvailable = true)
        : AssistantTool<HermesChatContext, EchoArguments>(Substitute.For<ILogger>())
    {
        public override string Name => "echo";

        public override string Description => "Echoes text.";

        public override bool IsAvailable(HermesChatContext context)
        {
            return isAvailable;
        }

        protected override ValueTask<AssistantToolResult> ExecuteAsync(
            HermesChatContext context,
            EchoArguments arguments,
            CancellationToken cancellationToken
        )
        {
            return ValueTask.FromResult(new AssistantToolResult($"echo: {arguments.Text}"));
        }
    }

    private GenerateCompletionHandler CreateHandler(
        IEnumerable<IAssistantTool<HermesChatContext>> tools
    )
    {
        var resolver = new ToolSetResolver(
            Substitute.For<ILogger<ToolSetResolver>>(),
            _dbContextFactory,
            [new NativeToolSource(tools)]
        );

        return new GenerateCompletionHandler(_logger, _mlClient, _dbContextFactory, resolver);
    }

    private async Task<Id<ToolSet>> SeedToolSetAsync(
        string? instructions = null,
        string? additionalDescription = null
    )
    {
        var toolSetId = DatabaseExtensions.CreateId<ToolSet>();
        var toolSet = ToolSet.Create(
            toolSetId,
            "Echo set",
            null,
            instructions,
            [
                ToolSetTool.Create(
                    DatabaseExtensions.CreateId<ToolSetTool>(),
                    toolSetId,
                    ToolSource.Native,
                    "echo",
                    additionalDescription
                ),
            ]
        );

        await using var seedContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        await seedContext.SeedData(toolSet);

        return toolSetId;
    }

    private void CaptureRequest()
    {
        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Do<string>(i => _sentInstructions = i),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Do<IReadOnlyList<FunctionToolDto>>(t => _sentTools = t),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(CreateStream([Text("Ok")]));
    }

    private static async IAsyncEnumerable<ResponseStreamChunk> CreateStream(
        IEnumerable<ResponseStreamChunk> chunks,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return chunk;
        }
    }

    private static ResponseStreamChunk Text(string text)
    {
        return new ResponseStreamChunk(text, null, null);
    }

    private static ResponseStreamChunk Reasoning(string reasoning)
    {
        return new ResponseStreamChunk(null, reasoning, null);
    }

    private static ResponseStreamChunk Usage(int input, int output)
    {
        return new ResponseStreamChunk(
            null,
            null,
            new ChatCompletionUsage(input, output, input + output)
        );
    }

    private static ResponseStreamChunk Call(string callId, string arguments)
    {
        return new ResponseStreamChunk(
            null,
            null,
            null,
            new FunctionCallDto(callId, "echo", arguments)
        );
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

    private static ConversationInput CreateConversation(
        ModelInput? model = null,
        params CompletionMessageInput[] messages
    )
    {
        return new(
            model ?? new ModelInput("test-model", "You are a helpful assistant."),
            new PersonaInput("TestBot", "A test persona"),
            [.. messages]
        );
    }

    private static GenerateCompletionInput CreateCommand(Id<ToolSet>? toolSetId)
    {
        return new GenerateCompletionInput(
            CreateConversation(
                new ModelInput("test-model", "Base prompt."),
                new CompletionMessageInput("Hi", Role.User)
            ) with
            {
                ToolSetId = toolSetId,
            }
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

    private async Task<Id<Conversation>> SeedConversationAsync()
    {
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var conversation = Conversation.Create(
            conversationId,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            "Test"
        );

        await using var seedContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        await seedContext.SeedData(conversation);

        return conversationId;
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldStreamInstructionsThenReplyThenDone()
    {
        // Arrange
        SetupStream(CreateStream([Reasoning("Hmm"), Text("Hello"), Text(" world"), Usage(10, 4)]));

        var command = new GenerateCompletionInput(
            CreateConversation(messages: new CompletionMessageInput("Hi", Role.User))
        );

        // Act
        var events = await CollectAsync(_handler.Handle(command, CancellationToken.None));

        // Assert
        var instructions = events[0].ShouldBeOfType<AssistantInstructionsEvent>();
        instructions.Instructions.ShouldContain("## Persona");
        instructions.Instructions.ShouldContain("## Instructions");
        instructions.Instructions.ShouldNotContain(GenerateCompletionHandler.ToolInstructions);

        events
            .Skip(1)
            .ShouldBe([
                new AssistantReasoningEvent("Hmm"),
                new AssistantContentEvent("Hello"),
                new AssistantContentEvent(" world"),
                new AssistantUsageEvent(10, 4, 14, 14),
                new AssistantDoneEvent(),
            ]);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldSendTheModelAndConversationSettingsAndComposedPrompt()
    {
        // Arrange
        SetupStream(CreateStream([Text("Ok")]));

        var model = new ModelInput("test-model", "Base prompt.", 0.4f);

        var command = new GenerateCompletionInput(
            CreateConversation(model, new CompletionMessageInput("Hi", Role.User)) with
            {
                ReasoningEffort = ReasoningEffort.High,
            }
        );

        // Act
        await CollectAsync(_handler.Handle(command, CancellationToken.None));

        // Assert
        _mlClient
            .Received(1)
            .StreamResponseAsync(
                "test-model",
                Arg.Is<string>(i =>
                    i.Contains("Base prompt.")
                    && !i.Contains(GenerateCompletionHandler.ToolInstructions)
                ),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Is<IReadOnlyList<FunctionToolDto>>(t => t.Count == 0),
                0.4f,
                ModelDefaults.MaxTokens,
                ReasoningEffort.High,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenNoToolSetIsSelected_ShouldNotOfferToolsAndSendOnlyTheSystemPrompt()
    {
        // Arrange
        CaptureRequest();

        var handler = CreateHandler([new EchoTool()]);
        var command = CreateCommand(null);

        // Act
        var events = await CollectAsync(handler.Handle(command, CancellationToken.None));

        // Assert
        _sentTools.ShouldNotBeNull();
        _sentTools.ShouldBeEmpty();
        _sentInstructions.ShouldNotBeNull();
        _sentInstructions.ShouldBe(
            GenerateCompletionHandler.ComposeSystemPrompt(command.Conversation)
        );
        events[0].ShouldBe(new AssistantInstructionsEvent(_sentInstructions));
    }

    [Fact]
    public async Task Handle_WhenToolSetHasAnAvailableTool_ShouldOfferItWithToolInstructions()
    {
        // Arrange
        CaptureRequest();

        var toolSetId = await SeedToolSetAsync();
        var handler = CreateHandler([new EchoTool()]);
        var command = CreateCommand(toolSetId);

        // Act
        var events = await CollectAsync(handler.Handle(command, CancellationToken.None));

        // Assert
        _sentTools.ShouldNotBeNull();
        var tool = _sentTools.ShouldHaveSingleItem();
        tool.Name.ShouldBe("echo");
        tool.Description.ShouldBe("Echoes text.");
        _sentInstructions.ShouldEndWith($"\n\n{GenerateCompletionHandler.ToolInstructions}");
        events[0].ShouldBe(new AssistantInstructionsEvent(_sentInstructions));
    }

    [Fact]
    public async Task Handle_WhenToolSetEntryHasAdditionalDescription_ShouldAppendItToTheToolDescription()
    {
        // Arrange
        CaptureRequest();

        var toolSetId = await SeedToolSetAsync(additionalDescription: "Only echo greetings.");
        var handler = CreateHandler([new EchoTool()]);

        // Act
        await CollectAsync(handler.Handle(CreateCommand(toolSetId), CancellationToken.None));

        // Assert
        _sentTools.ShouldNotBeNull();
        _sentTools
            .ShouldHaveSingleItem()
            .Description.ShouldBe("Echoes text.\n\nOnly echo greetings.");
    }

    [Fact]
    public async Task Handle_WhenToolSetHasInstructions_ShouldAppendThemAfterTheToolInstructions()
    {
        // Arrange
        CaptureRequest();

        var toolSetId = await SeedToolSetAsync(instructions: "Echo whenever asked.");
        var handler = CreateHandler([new EchoTool()]);

        // Act
        var events = await CollectAsync(
            handler.Handle(CreateCommand(toolSetId), CancellationToken.None)
        );

        // Assert
        var instructions = events[0].ShouldBeOfType<AssistantInstructionsEvent>().Instructions;
        instructions.ShouldEndWith(
            $"\n\n{GenerateCompletionHandler.ToolInstructions}\nEcho whenever asked."
        );
        _sentInstructions.ShouldBe(instructions);
    }

    [Fact]
    public async Task Handle_WhenToolSetDoesNotExist_ShouldNotOfferTools()
    {
        // Arrange
        CaptureRequest();

        var handler = CreateHandler([new EchoTool()]);
        var command = CreateCommand(DatabaseExtensions.CreateId<ToolSet>());

        // Act
        await CollectAsync(handler.Handle(command, CancellationToken.None));

        // Assert
        _sentTools.ShouldNotBeNull();
        _sentTools.ShouldBeEmpty();
        _sentInstructions.ShouldNotBeNull();
        _sentInstructions.ShouldNotContain(GenerateCompletionHandler.ToolInstructions);
    }

    [Fact]
    public async Task Handle_WhenToolSetToolIsUnavailable_ShouldFilterItOut()
    {
        // Arrange
        CaptureRequest();

        var toolSetId = await SeedToolSetAsync(instructions: "Echo whenever asked.");
        var handler = CreateHandler([new EchoTool(isAvailable: false)]);
        var command = CreateCommand(toolSetId);

        // Act
        await CollectAsync(handler.Handle(command, CancellationToken.None));

        // Assert
        _sentTools.ShouldNotBeNull();
        _sentTools.ShouldBeEmpty();
        _sentInstructions.ShouldNotBeNull();
        _sentInstructions.ShouldBe(
            GenerateCompletionHandler.ComposeSystemPrompt(command.Conversation)
        );
    }

    [Fact]
    public async Task Handle_WhenLorasAreSelected_ShouldPassThemToTheToolContext()
    {
        // Arrange
        SetupStream(CreateStream([Text("Ok")]));
        HermesChatContext? context = null;
        var tool = Substitute.For<IAssistantTool<HermesChatContext>>();
        tool.Name.Returns("echo");
        tool.IsAvailable(Arg.Do<HermesChatContext>(c => context = c)).Returns(false);

        var toolSetId = await SeedToolSetAsync();
        var handler = CreateHandler([tool]);
        var conversation = CreateCommand(toolSetId).Conversation with
        {
            Loras = [new LoraInput("add-detail", 0.8f)],
        };

        // Act
        await CollectAsync(
            handler.Handle(new GenerateCompletionInput(conversation), CancellationToken.None)
        );

        // Assert
        context.ShouldNotBeNull();
        context.ImageLoras.ShouldBe([new ImageLora("add-detail", 0.8f)]);
    }

    [Fact]
    public async Task Handle_WhenHistoryHasToolCalls_ShouldReplayThemBeforeTheReply()
    {
        // Arrange
        IReadOnlyList<ResponseInputItemDto>? sentInput = null;

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Do<IReadOnlyList<ResponseInputItemDto>>(i => sentInput = i),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(CreateStream([Text("Ok")]));

        var command = new GenerateCompletionInput(
            CreateConversation(
                null,
                new CompletionMessageInput("Echo hi", Role.User),
                new CompletionMessageInput(
                    "It said hi.",
                    Role.Assistant,
                    [new CompletionToolCallInput("call-1", "echo", """{"text":"hi"}""", "echo: hi")]
                ),
                new CompletionMessageInput("Earlier summary", Role.Summary),
                new CompletionMessageInput("Again", Role.User)
            )
        );

        // Act
        await CollectAsync(_handler.Handle(command, CancellationToken.None));

        // Assert
        sentInput.ShouldNotBeNull();
        sentInput.ShouldBe([
            new MessageInputItemDto(new MessageDto("Echo hi", RoleDto.User)),
            new FunctionCallInputItemDto(
                new FunctionCallDto("call-1", "echo", """{"text":"hi"}""")
            ),
            new FunctionCallOutputInputItemDto("call-1", "echo: hi"),
            new MessageInputItemDto(new MessageDto("It said hi.", RoleDto.Assistant)),
            new MessageInputItemDto(new MessageDto("Earlier summary", RoleDto.Assistant)),
            new MessageInputItemDto(new MessageDto("Again", RoleDto.User)),
        ]);
    }

    [Fact]
    public void BuildInput_WhenReplayedToolOutputIsTooLong_ShouldTruncateIt()
    {
        // Arrange
        CompletionMessageInput[] messages =
        [
            new(
                "Done.",
                Role.Assistant,
                [new CompletionToolCallInput("call-1", "echo", "{}", new string('a', 10))]
            ),
        ];

        // Act
        var input = GenerateCompletionHandler.BuildInput(messages, 4);

        // Assert
        input[1].ShouldBe(new FunctionCallOutputInputItemDto("call-1", "aaaa…"));
    }

    [Fact]
    public void BuildInput_WhenAReplyHasNoText_ShouldReplayOnlyItsToolCalls()
    {
        // Arrange
        CompletionMessageInput[] messages =
        [
            new(
                "",
                Role.Assistant,
                [new CompletionToolCallInput("call-1", "generate_image", "{}", "Shown.")]
            ),
        ];

        // Act
        var input = GenerateCompletionHandler.BuildInput(messages, 100);

        // Assert
        input.ShouldBe([
            new FunctionCallInputItemDto(new FunctionCallDto("call-1", "generate_image", "{}")),
            new FunctionCallOutputInputItemDto("call-1", "Shown."),
        ]);
    }

    [Fact]
    public async Task Handle_WhenConversationIdProvided_ShouldPersistTheTurnWithOrderedParts()
    {
        // Arrange
        var conversationId = await SeedConversationAsync();
        var toolSetId = await SeedToolSetAsync();

        SetupStream(
            CreateStream([
                Reasoning("Let me check"),
                Call("call-1", """{"text":"hi"}"""),
                Usage(10, 5),
            ]),
            CreateStream([Text("It said "), Text("hi."), Usage(30, 3)])
        );

        var handler = CreateHandler([new EchoTool()]);
        var command = new GenerateCompletionInput(
            CreateConversation(
                new ModelInput("test-model", "Base prompt."),
                new CompletionMessageInput("Echo hi", Role.User)
            ) with
            {
                ToolSetId = toolSetId,
            },
            conversationId
        );

        // Act
        await CollectAsync(handler.Handle(command, CancellationToken.None));

        // Assert
        await using var checkContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var messages = await checkContext
            .Messages.Include(m => m.Parts)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        messages
            .Select(m => (m.Role, m.Content, m.SortOrder))
            .ShouldBe([(Role.User, "Echo hi", 0), (Role.Assistant, "It said hi.", 1)]);

        var parts = messages[1].Parts.OrderBy(p => p.SortOrder).ToList();
        parts[0].ShouldBeOfType<ReasoningMessagePart>().Content.ShouldBe("Let me check");
        var tool = parts[1].ShouldBeOfType<ToolCallMessagePart>();
        tool.CallId.ShouldBe("call-1");
        tool.Status.ShouldBe(AssistantToolStatus.Completed);
        tool.Output.ShouldBe("echo: hi");
        parts[2].ShouldBeOfType<TextMessagePart>().Content.ShouldBe("It said hi.");

        var conversation = await checkContext.Conversations.SingleAsync();
        conversation.InputTokenCount.ShouldBe(40);
        conversation.OutputTokenCount.ShouldBe(8);
        conversation.TotalTokenCount.ShouldBe(33);
    }

    [Fact]
    public async Task Handle_WhenGenerationFails_ShouldStopWithErrorAndPersistNothing()
    {
        // Arrange
        var conversationId = await SeedConversationAsync();

        SetupStream(CreateStream([Reasoning("Thinking"), Usage(10, 256)]));

        var command = new GenerateCompletionInput(
            CreateConversation(messages: new CompletionMessageInput("Hello", Role.User)),
            conversationId
        );

        // Act
        var events = await CollectAsync(_handler.Handle(command, CancellationToken.None));

        // Assert
        events
            .Last()
            .ShouldBe(
                new AssistantErrorEvent(
                    AssistantToolLoop<HermesChatContext>.GenerationFailedMessage
                )
            );
        events.ShouldNotContain(e => e is AssistantDoneEvent);

        await using var checkContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        (await checkContext.Messages.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenConversationIdNull_ShouldNotPersist()
    {
        // Arrange
        SetupStream(CreateStream([Text("Hi")]));

        var command = new GenerateCompletionInput(
            CreateConversation(messages: new CompletionMessageInput("Hello", Role.User))
        );

        // Act
        await CollectAsync(_handler.Handle(command, CancellationToken.None));

        // Assert
        await _dbContextFactory.DidNotReceive().CreateDbContextAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConversationDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _dbContextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
                Task.FromResult(
                    DbContextExtensions.Mock<HermesDbContext>(Guid.NewGuid().ToString())
                )
            );

        var command = new GenerateCompletionInput(
            CreateConversation(messages: new CompletionMessageInput("Hello", Role.User)),
            new Id<Conversation>(Guid.NewGuid().ToString())
        );

        // Act & Assert
        await Should.ThrowAsync<Ardalis.GuardClauses.NotFoundException>(() =>
            CollectAsync(_handler.Handle(command, CancellationToken.None))
        );
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new GenerateCompletionInput(CreateConversation());
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () =>
        {
            await foreach (var _ in _handler.Handle(command, cancellationToken)) { }
        };

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Constructor_WhenToolSetResolverIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GenerateCompletionHandler(_logger, _mlClient, _dbContextFactory, null!)
        );
    }

    [Fact]
    public void ComposeSystemPrompt_WhenFullPersona_ShouldIncludeAllFields()
    {
        // Arrange
        var persona = new PersonaInput("TestBot", "A helpful bot", "Friendly", "In a chat");
        var conversation = new ConversationInput(
            new ModelInput("test-model", "You are an assistant."),
            persona,
            [new CompletionMessageInput("Hi", Role.User)]
        );

        // Act
        var result = GenerateCompletionHandler.ComposeSystemPrompt(conversation);

        // Assert
        result.ShouldContain("## Persona");
        result.ShouldContain("**Name:** TestBot");
        result.ShouldContain("**Description:** A helpful bot");
        result.ShouldContain("**Personality:** Friendly");
        result.ShouldContain("**Scenario:** In a chat");
        result.ShouldContain("## Instructions");
        result.ShouldContain("You are an assistant.");
    }

    [Fact]
    public void ComposeSystemPrompt_WhenTraitsProvided_ShouldIncludeTraitSection()
    {
        // Arrange
        var conversation = new ConversationInput(
            new ModelInput("test-model", "Base prompt."),
            new PersonaInput("TestBot", "A helpful bot"),
            [new CompletionMessageInput("Hi", Role.User)],
            [new TraitInput("Kindness", "Always be kind"), new TraitInput("Brevity", "Be concise")]
        );

        // Act
        var result = GenerateCompletionHandler.ComposeSystemPrompt(conversation);

        // Assert
        result.ShouldContain("## Traits");
        result.ShouldContain("### Kindness");
        result.ShouldContain("Always be kind");
        result.ShouldContain("### Brevity");
        result.ShouldContain("Be concise");
    }

    [Fact]
    public void ComposeSystemPrompt_WhenNoTraits_ShouldOmitTraitSection()
    {
        // Arrange
        var conversation = new ConversationInput(
            new ModelInput("test-model", "Base prompt."),
            new PersonaInput("TestBot", "A helpful bot"),
            [new CompletionMessageInput("Hi", Role.User)]
        );

        // Act
        var result = GenerateCompletionHandler.ComposeSystemPrompt(conversation);

        // Assert
        result.ShouldNotContain("## Traits");
    }

    [Fact]
    public void ComposeInstructions_WhenToolSetHasNoTools_ShouldReturnTheSystemPrompt()
    {
        // Arrange
        var toolSet = new ResolvedToolSet([], "Use tools wisely.");

        // Act
        var result = GenerateCompletionHandler.ComposeInstructions("Prompt.", toolSet);

        // Assert
        result.ShouldBe("Prompt.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ComposeInstructions_WhenToolSetHasToolsButNoInstructions_ShouldAppendOnlyToolInstructions(
        string? instructions
    )
    {
        // Arrange
        var toolSet = new ResolvedToolSet([new EchoTool()], instructions);

        // Act
        var result = GenerateCompletionHandler.ComposeInstructions("Prompt.", toolSet);

        // Assert
        result.ShouldBe($"Prompt.\n\n{GenerateCompletionHandler.ToolInstructions}");
    }

    [Fact]
    public void ComposeInstructions_WhenToolSetHasToolsAndInstructions_ShouldAppendBoth()
    {
        // Arrange
        var toolSet = new ResolvedToolSet([new EchoTool()], "Use tools wisely.");

        // Act
        var result = GenerateCompletionHandler.ComposeInstructions("Prompt.", toolSet);

        // Assert
        result.ShouldBe(
            $"Prompt.\n\n{GenerateCompletionHandler.ToolInstructions}\nUse tools wisely."
        );
    }
}
