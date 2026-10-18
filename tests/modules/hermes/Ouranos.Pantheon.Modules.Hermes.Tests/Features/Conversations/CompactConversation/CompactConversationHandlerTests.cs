using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.CompactConversation;

public sealed class CompactConversationHandlerTests
{
    private readonly Fixture _fixture = new();

    private readonly ILogger<CompactConversationHandler> _logger = Substitute.For<
        ILogger<CompactConversationHandler>
    >();

    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();

    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory = Substitute.For<
        IDbContextFactory<HermesDbContext>
    >();

    private readonly IOptions<HermesOptions> _options = Options.Create(new HermesOptions());

    private readonly CompactConversationHandler _handler;

    public CompactConversationHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _handler = new CompactConversationHandler(_logger, _mlClient, _dbContextFactory, _options);
    }

    private static async IAsyncEnumerable<ResponseStreamChunk> CreateStream(
        IEnumerable<string> chunks,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return new ResponseStreamChunk(chunk, null, null);
        }
    }

    private static async IAsyncEnumerable<ResponseStreamChunk> CreateStreamWithUsage(
        IEnumerable<string> chunks,
        ChatCompletionUsage usage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return new ResponseStreamChunk(chunk, null, null);
        }

        yield return new ResponseStreamChunk(null, null, usage);
    }

    private static async IAsyncEnumerable<ResponseStreamChunk> CreateFailingStream(
        IEnumerable<string> chunks,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (var chunk in CreateStream(chunks, cancellationToken))
        {
            yield return chunk;
        }

        throw new HttpRequestException("Inference host unavailable.");
    }

    private static CompactConversationInput CreateCommand(
        Id<Conversation>? conversationId = null,
        string? personaName = null,
        string? personaDescription = null,
        List<CompactConversationMessageInput>? messages = null
    )
    {
        return new CompactConversationInput(
            ConversationId: conversationId,
            ModelIdentifier: "test-model",
            PersonaName: personaName ?? "TestBot",
            PersonaDescription: personaDescription ?? "A test persona",
            Messages: messages ?? []
        );
    }

    private static bool ContainsAfterLastSummaryOnly(string instructions)
    {
        var systemContent = instructions;
        return systemContent.Contains("What about X?")
            && systemContent.Contains("Here is info about X.")
            && systemContent.Contains("And more.")
            && !systemContent.Contains("EARLY SUMMARY")
            && !systemContent.Contains("Hello")
            && !systemContent.Contains("Hi!");
    }

    private static Conversation CreateConversationWithMessages(
        Id<Conversation> conversationId,
        params Message[] messages
    )
    {
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());

        var conversation = Conversation.Create(
            conversationId,
            personaId,
            modelConfigId,
            messages,
            [],
            name: "Test Conversation"
        );

        return conversation;
    }

    [Fact]
    public async Task Handle_WhenConversationIdProvided_ShouldPersistSummaryMessage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        _dbContextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(DbContextExtensions.Mock<HermesDbContext>(dbName)));

        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());

        var conversation = CreateConversationWithMessages(conversationId);
        var dbContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        await dbContext.Conversations.AddAsync(conversation);
        await dbContext.SaveChangesAsync();

        var messages = new List<CompactConversationMessageInput>
        {
            new("Hello", Role.User),
            new("Hi there!", Role.Assistant),
        };

        var command = CreateCommand(conversationId: conversationId, messages: messages);
        var expectedSummary = "Concise summary of the conversation.";

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
            .Returns(CreateStream([expectedSummary]));

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        results.ShouldContain(r => r is CompactContentChunkResponse);
        var contentChunks = results.OfType<CompactContentChunkResponse>().ToList();
        contentChunks.Count.ShouldBe(1);
        contentChunks[0].Content.ShouldBe(expectedSummary);

        var complete = results.OfType<CompactCompleteResponse>().SingleOrDefault();
        complete.ShouldNotBeNull();
        complete.SummaryMessageId.ShouldNotBeNull();

        var checkContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        var savedSummary = await checkContext
            .Messages.Where(m => m.ConversationId == conversationId && m.Role == Role.Summary)
            .FirstOrDefaultAsync();

        savedSummary.ShouldNotBeNull();
        savedSummary.Id.ShouldBe(complete.SummaryMessageId!.Value);
        savedSummary.Content.ShouldBe(expectedSummary);
        savedSummary.SortOrder.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenConversationIdNull_ShouldReturnSummaryWithoutPersisting()
    {
        // Arrange
        var messages = new List<CompactConversationMessageInput>
        {
            new("Hello", Role.User),
            new("How can I help?", Role.Assistant),
        };

        var command = CreateCommand(messages: messages);
        var expectedSummary = "Ephemeral summary.";

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
            .Returns(CreateStream([expectedSummary]));

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        var contentChunks = results.OfType<CompactContentChunkResponse>().ToList();
        contentChunks.Count.ShouldBe(1);
        contentChunks[0].Content.ShouldBe(expectedSummary);

        var complete = results.OfType<CompactCompleteResponse>().SingleOrDefault();
        complete.ShouldNotBeNull();
        complete.SummaryMessageId.ShouldBeNull();

        await _dbContextFactory.DidNotReceive().CreateDbContextAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoUserOrAssistantMessages_ShouldYieldErrorEvent()
    {
        // Arrange
        var messages = new List<CompactConversationMessageInput>
        {
            new("System instruction", Role.System),
        };

        var command = CreateCommand(messages: messages);

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        results.ShouldHaveSingleItem();
        results[0]
            .ShouldBeOfType<CompactErrorResponse>()
            .Message.ShouldBe(CompactConversationHandler.NothingToCompactMessage);

        _mlClient
            .DidNotReceive()
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenStreamThrows_ShouldYieldErrorEventWithoutPersisting()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        _dbContextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(DbContextExtensions.Mock<HermesDbContext>(dbName)));

        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var dbContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        await dbContext.Conversations.AddAsync(CreateConversationWithMessages(conversationId));
        await dbContext.SaveChangesAsync();

        var command = CreateCommand(
            conversationId: conversationId,
            messages: [new("Hello", Role.User), new("Hi!", Role.Assistant)]
        );

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
            .Returns(CreateFailingStream(["Partial "]));

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        results
            .Last()
            .ShouldBeOfType<CompactErrorResponse>()
            .Message.ShouldBe(CompactConversationHandler.CompactionFailedMessage);
        results.ShouldNotContain(r => r is CompactCompleteResponse);

        var checkContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        (
            await checkContext.Messages.AnyAsync(m => m.ConversationId == conversationId)
        ).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenSummaryIsEmpty_ShouldYieldErrorEventWithoutPersisting()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        _dbContextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(DbContextExtensions.Mock<HermesDbContext>(dbName)));

        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var dbContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        await dbContext.Conversations.AddAsync(CreateConversationWithMessages(conversationId));
        await dbContext.SaveChangesAsync();

        var command = CreateCommand(
            conversationId: conversationId,
            messages: [new("Hello", Role.User), new("Hi!", Role.Assistant)]
        );

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
            .Returns(
                CreateStreamWithUsage(
                    ["  "],
                    new ChatCompletionUsage(InputTokens: 100, OutputTokens: 4096, TotalTokens: 4196)
                )
            );

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        results
            .Last()
            .ShouldBeOfType<CompactErrorResponse>()
            .Message.ShouldBe(CompactConversationHandler.CompactionFailedMessage);
        results.ShouldNotContain(r => r is CompactCompleteResponse);
        results.ShouldNotContain(r => r is CompactUsageChunkResponse);

        var checkContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        (
            await checkContext.Messages.AnyAsync(m => m.ConversationId == conversationId)
        ).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenUsageReported_ShouldRecordSummarySizeAsTokenUsage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        _dbContextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(DbContextExtensions.Mock<HermesDbContext>(dbName)));

        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var dbContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        await dbContext.Conversations.AddAsync(CreateConversationWithMessages(conversationId));
        await dbContext.SaveChangesAsync();

        var command = CreateCommand(
            conversationId: conversationId,
            messages: [new("Hello", Role.User), new("Hi!", Role.Assistant)]
        );

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
            .Returns(
                CreateStreamWithUsage(
                    ["Summary."],
                    new ChatCompletionUsage(InputTokens: 9000, OutputTokens: 300, TotalTokens: 9300)
                )
            );

        // Act
        await foreach (var _ in _handler.Handle(command, CancellationToken.None)) { }

        // Assert
        var checkContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        var conversation = await checkContext.Conversations.SingleAsync(c =>
            c.Id == conversationId
        );

        conversation.InputTokenCount.ShouldBe(0);
        conversation.OutputTokenCount.ShouldBe(300);
        conversation.TotalTokenCount.ShouldBe(300);
    }

    [Fact]
    public async Task Handle_WhenExistingSummaryInMessages_ShouldSummarizeOnlyAfterLastSummary()
    {
        // Arrange
        var messages = new List<CompactConversationMessageInput>
        {
            new("Hello", Role.User),
            new("Hi!", Role.Assistant),
            new("EARLY SUMMARY", Role.Summary),
            new("What about X?", Role.User),
            new("Here is info about X.", Role.Assistant),
            new("And more.", Role.User),
        };

        var command = CreateCommand(messages: messages);
        var expectedSummary = "Summary of messages after last summary.";

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
            .Returns(CreateStream([expectedSummary]));

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        _mlClient
            .Received(1)
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Is<string>(i => ContainsAfterLastSummaryOnly(i)),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            );

        var complete = results.OfType<CompactCompleteResponse>().SingleOrDefault();
        complete.ShouldNotBeNull();
        complete.SummaryMessageId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = CreateCommand(
            messages: [new("Hello", Role.User), new("Hi!", Role.Assistant)]
        );

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
    public async Task Handle_WhenStreaming_ShouldYieldContentChunks()
    {
        // Arrange
        var chunks = new[] { "First ", "chunk ", "and more." };

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
            .Returns(CreateStream(chunks));

        var command = CreateCommand(
            messages: [new("Hello", Role.User), new("Hi!", Role.Assistant)]
        );

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        var contentChunks = results.OfType<CompactContentChunkResponse>().ToList();
        contentChunks.Count.ShouldBe(3);
        contentChunks[0].Content.ShouldBe("First ");
        contentChunks[1].Content.ShouldBe("chunk ");
        contentChunks[2].Content.ShouldBe("and more.");
    }

    [Fact]
    public async Task Handle_WhenStreamingWithUsage_ShouldYieldUsageChunk()
    {
        // Arrange
        var usage = new ChatCompletionUsage(InputTokens: 100, OutputTokens: 50, TotalTokens: 150);

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
            .Returns(CreateStreamWithUsage(["Hello"], usage));

        var command = CreateCommand(messages: [new("Hi", Role.User)]);

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        var usageResponse = results.OfType<CompactUsageChunkResponse>().SingleOrDefault();
        usageResponse.ShouldNotBeNull();
        usageResponse.InputTokens.ShouldBe(100);
        usageResponse.OutputTokens.ShouldBe(50);
        usageResponse.TotalTokens.ShouldBe(150);
    }

    [Fact]
    public async Task Handle_WhenStreamingWithoutUsage_ShouldNotYieldUsageChunk()
    {
        // Arrange
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
            .Returns(CreateStream(["Hello"]));

        var command = CreateCommand(messages: [new("Hi", Role.User)]);

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        results.ShouldNotContain(r => r is CompactUsageChunkResponse);
    }

    [Fact]
    public void ComposeSummaryPrompt_WhenCalled_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new CompactConversationInput(
            null,
            "test-model",
            "AssistantBot",
            "A friendly assistant",
            []
        );

        var messages = new List<CompactConversationMessageInput>
        {
            new("Hello!", Role.User),
            new("How can I help?", Role.Assistant),
            new("Tell me about X.", Role.User),
        };

        // Act
        var result = CompactConversationHandler.ComposeSummaryPrompt(
            command,
            messages,
            HermesOptions.DefaultCompactionSummaryPrompt,
            HermesOptions.DefaultCompactionMaxInputCharacters
        );

        // Assert
        result.ShouldContain("You are summarizing a conversation for context compaction.");
        result.ShouldContain("AssistantBot: A friendly assistant");
        result.ShouldContain("User: Hello!");
        result.ShouldContain("AssistantBot: How can I help?");
        result.ShouldContain("User: Tell me about X.");
    }

    [Fact]
    public void ComposeSummaryPrompt_WhenCustomPromptProvided_ShouldUseCustomPrompt()
    {
        // Arrange
        var customPrompt =
            "Custom prompt for {PersonaName} with {PersonaDescription}. Conversation history:";
        var command = new CompactConversationInput(
            null,
            "test-model",
            "CustomBot",
            "A custom persona",
            []
        );

        var messages = new List<CompactConversationMessageInput> { new("Hello!", Role.User) };

        // Act
        var result = CompactConversationHandler.ComposeSummaryPrompt(
            command,
            messages,
            customPrompt,
            HermesOptions.DefaultCompactionMaxInputCharacters
        );

        // Assert
        result.ShouldContain("Custom prompt for CustomBot with A custom persona.");
        result.ShouldContain("User: Hello!");
    }

    [Fact]
    public async Task Handle_WhenCustomOptionsProvided_ShouldPassThemToMlClient()
    {
        // Arrange
        var customOptions = Options.Create(
            new HermesOptions(
                ConversationNameSystemPrompt: "name prompt",
                ConversationNameModel: "name-model",
                ConversationNameReasoningEffort: null,
                CompactionSummaryPrompt: "Custom system prompt.",
                CompactionTemperature: 0.7f,
                CompactionMaxTokens: 2048,
                CompactionReasoningEffort: ReasoningEffort.High,
                ImageGeneration: new HermesImageGenerationOptions()
            )
        );

        var handler = new CompactConversationHandler(
            _logger,
            _mlClient,
            _dbContextFactory,
            customOptions
        );

        var messages = new List<CompactConversationMessageInput>
        {
            new("Hello", Role.User),
            new("Hi!", Role.Assistant),
        };

        var command = CreateCommand(messages: messages);
        var expectedSummary = "Custom summary.";

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
            .Returns(CreateStream([expectedSummary]));

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        _mlClient
            .Received(1)
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Is<string>(i => i.Contains("Custom system prompt.")),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Is<IReadOnlyList<FunctionToolDto>>(t => t.Count == 0),
                Arg.Is<float?>(t => t == 0.7f),
                Arg.Is<int?>(m => m == 2048),
                ReasoningEffort.High,
                Arg.Any<CancellationToken>()
            );

        var contentChunks = results.OfType<CompactContentChunkResponse>().ToList();
        contentChunks.Count.ShouldBe(1);
        contentChunks[0].Content.ShouldBe(expectedSummary);
    }

    [Fact]
    public async Task Handle_WhenModelReasons_ShouldStreamOnlyTheSummaryText()
    {
        // Arrange
        var command = CreateCommand(
            messages: [new("Hello", Role.User), new("Hi!", Role.Assistant)]
        );

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
            .Returns(
                new[]
                {
                    new ResponseStreamChunk(null, "Planning the summary", null),
                    new ResponseStreamChunk("Summary.", null, null),
                }.ToAsyncEnumerable()
            );

        // Act
        var results = new List<CompactConversationResponse>();
        await foreach (var chunk in _handler.Handle(command, CancellationToken.None))
        {
            results.Add(chunk);
        }

        // Assert
        results
            .OfType<CompactContentChunkResponse>()
            .Select(c => c.Content)
            .ShouldBe(["Summary."]);

        _mlClient
            .Received(1)
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Is<IReadOnlyList<FunctionToolDto>>(t => t.Count == 0),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                ReasoningEffort.Low,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public void ComposeSummaryPrompt_WhenHistoryExceedsBudget_ShouldKeepNewestMessagesWithinBudget()
    {
        // Arrange
        var command = new CompactConversationInput(null, "test-model", "Bot", "A bot", []);

        var messages = new List<CompactConversationMessageInput>
        {
            new("oldest-" + new string('a', 50), Role.User),
            new("middle-" + new string('b', 50), Role.Assistant),
            new("newest-" + new string('c', 50), Role.User),
        };

        // Act
        var result = CompactConversationHandler.ComposeSummaryPrompt(
            command,
            messages,
            "Template.",
            150
        );

        // Assert
        result.ShouldNotContain("oldest-");
        result.ShouldContain("Bot: middle-");
        result.ShouldContain("User: newest-");
        result.ShouldContain("(Earlier messages omitted.)");
        result
            .IndexOf("middle-", StringComparison.Ordinal)
            .ShouldBeLessThan(result.IndexOf("newest-", StringComparison.Ordinal));
    }

    [Fact]
    public void ComposeSummaryPrompt_WhenNewestMessageExceedsBudget_ShouldTruncateIt()
    {
        // Arrange
        var command = new CompactConversationInput(null, "test-model", "Bot", "A bot", []);

        var messages = new List<CompactConversationMessageInput>
        {
            new("older", Role.Assistant),
            new(new string('x', 500), Role.User),
        };

        // Act
        var result = CompactConversationHandler.ComposeSummaryPrompt(
            command,
            messages,
            "Template.",
            100
        );

        // Assert
        var historyLine = result
            .Split(Environment.NewLine)
            .Single(line => line.StartsWith("User: ", StringComparison.Ordinal));
        historyLine.Length.ShouldBe(100);
        historyLine.ShouldEndWith("…");
        result.ShouldNotContain("Bot: older");
    }

    [Fact]
    public void ComposeSummaryPrompt_WhenHistoryFitsBudget_ShouldNotMarkOmission()
    {
        // Arrange
        var command = new CompactConversationInput(null, "test-model", "Bot", "A bot", []);

        var messages = new List<CompactConversationMessageInput> { new("Hello!", Role.User) };

        // Act
        var result = CompactConversationHandler.ComposeSummaryPrompt(
            command,
            messages,
            "Template.",
            HermesOptions.DefaultCompactionMaxInputCharacters
        );

        // Assert
        result.ShouldContain("User: Hello!");
        result.ShouldNotContain("(Earlier messages omitted.)");
    }

    [Fact]
    public async Task Handle_WhenMaxInputCharactersConfigured_ShouldBoundThePrompt()
    {
        // Arrange
        var customOptions = Options.Create(
            new HermesOptions() with
            {
                CompactionMaxInputCharacters = 40,
            }
        );

        var handler = new CompactConversationHandler(
            _logger,
            _mlClient,
            _dbContextFactory,
            customOptions
        );

        var command = CreateCommand(
            messages:
            [
                new("ancient history " + new string('z', 100), Role.User),
                new("recent", Role.Assistant),
            ]
        );

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
            .Returns(CreateStream(["Summary."]));

        // Act
        await foreach (var _ in handler.Handle(command, CancellationToken.None)) { }

        // Assert
        _mlClient
            .Received(1)
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Is<string>(i =>
                    i.Contains("TestBot: recent") && !i.Contains("ancient history")
                ),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            );
    }
}
