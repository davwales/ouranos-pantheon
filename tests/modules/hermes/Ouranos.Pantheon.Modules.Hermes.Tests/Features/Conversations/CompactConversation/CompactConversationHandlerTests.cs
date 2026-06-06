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
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.CompactConversation;

public sealed class CompactConversationHandlerTests
{
    private readonly IFixture _fixture = new Fixture();

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

    private static async IAsyncEnumerable<ChatCompletionChunk> CreateStream(
        IEnumerable<string> chunks,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return new ChatCompletionChunk(chunk, null);
        }
    }

    private static async IAsyncEnumerable<ChatCompletionChunk> CreateStreamWithUsage(
        IEnumerable<string> chunks,
        ChatCompletionUsage usage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return new ChatCompletionChunk(chunk, null);
        }

        yield return new ChatCompletionChunk(null, usage);
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
            SystemPrompt: "You are a helpful assistant.",
            PersonaName: personaName ?? "TestBot",
            PersonaDescription: personaDescription ?? "A test persona",
            Messages: messages ?? []
        );
    }

    private static bool ContainsAfterLastSummaryOnly(List<MessageDto> msgs)
    {
        var systemContent = msgs[0].Content;
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
    public async Task Handle_WhenNoUserOrAssistantMessages_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var messages = new List<CompactConversationMessageInput>
        {
            new("System instruction", Role.System),
        };

        var command = CreateCommand(messages: messages);

        // Act
        var handle = async () =>
        {
            await foreach (var _ in _handler.Handle(command, CancellationToken.None)) { }
        };

        // Assert
        var exception = await handle.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe(
            "Cannot compact a conversation with no user or assistant messages to summarize."
        );
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Is<List<MessageDto>>(msgs => ContainsAfterLastSummaryOnly(msgs)),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
            "You are helpful.",
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
            HermesOptions.DefaultCompactionSummaryPrompt
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
            "You are helpful.",
            "CustomBot",
            "A custom persona",
            []
        );

        var messages = new List<CompactConversationMessageInput> { new("Hello!", Role.User) };

        // Act
        var result = CompactConversationHandler.ComposeSummaryPrompt(
            command,
            messages,
            customPrompt
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
                CompactionSummaryPrompt: "Custom system prompt.",
                CompactionTemperature: 0.7f,
                CompactionMaxTokens: 2048
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
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
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Is<List<MessageDto>>(msgs => msgs[0].Content.Contains("Custom system prompt.")),
                Arg.Is<float?>(t => t == 0.7f),
                Arg.Is<int?>(m => m == 2048),
                Arg.Any<float?>(),
                Arg.Any<CancellationToken>()
            );

        var contentChunks = results.OfType<CompactContentChunkResponse>().ToList();
        contentChunks.Count.ShouldBe(1);
        contentChunks[0].Content.ShouldBe(expectedSummary);
    }
}
