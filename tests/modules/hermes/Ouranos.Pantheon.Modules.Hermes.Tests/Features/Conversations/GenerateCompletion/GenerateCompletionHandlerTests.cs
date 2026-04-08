using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.Shouldly;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandlerTests
{
    private readonly ILogger<GenerateCompletionHandler> _logger = Substitute.For<ILogger<GenerateCompletionHandler>>();
    private readonly IOuranosMachineLearningClient _mlClient = Substitute.For<IOuranosMachineLearningClient>();

    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory =
        Substitute.For<IDbContextFactory<HermesDbContext>>();

    private readonly GenerateCompletionHandler _handler;

    public GenerateCompletionHandlerTests()
    {
        _handler = new GenerateCompletionHandler(_logger, _mlClient, _dbContextFactory);
    }

    private static async IAsyncEnumerable<string> CreateStream(
        IEnumerable<string> chunks,
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

    private static ConversationInput CreateConversation(params CompletionMessageInput[] messages) => new(
        new ModelInput("test-model", "You are a helpful assistant."),
        new PersonaInput("TestBot", "A test persona"),
        messages.ToList()
    );

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldStreamChunks()
    {
        // Arrange
        var chunks = new[] { "Hello", " world" };

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

        var command = new GenerateCompletionInput(CreateConversation(new CompletionMessageInput("Hi", Role.User)));
        var expected = chunks.Select(c => new GenerateCompletionResponse(c)).ToList();

        // Act + Assert
        await _handler.Handle(command, CancellationToken.None).ShouldMatchAsync(expected);
    }

    [Fact]
    public async Task Handle_WhenConversationIdProvided_ShouldPersistMessages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(dbContext));

        _mlClient
            .StreamChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(CreateStream(["Hi there"]));

        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var command = new GenerateCompletionInput(
            CreateConversation(new CompletionMessageInput("Hello", Role.User)),
            conversationId
        );

        // Act
        await foreach (var _ in _handler.Handle(command, CancellationToken.None))
        {
        }

        // Assert
        var checkContext = DbContextExtensions.Mock<HermesDbContext>(dbName);
        var messages = await checkContext.Messages.ToListAsync();
        messages.Count.ShouldBe(2);
        messages.ShouldContain(m => m.Role == Role.User);
        messages.ShouldContain(m => m.Role == Role.Assistant);
    }

    [Fact]
    public async Task Handle_WhenConversationIdNull_ShouldNotPersist()
    {
        // Arrange
        _mlClient.StreamChatCompletionAsync(
            Arg.Any<string>(),
            Arg.Any<List<MessageDto>>(),
            Arg.Any<float?>(),
            Arg.Any<int?>(),
            Arg.Any<float?>(),
            Arg.Any<CancellationToken>()
        ).Returns(CreateStream(["Hi"]));

        var command = new GenerateCompletionInput(CreateConversation(new CompletionMessageInput("Hello", Role.User)));

        // Act
        await foreach (var _ in _handler.Handle(command, CancellationToken.None))
        {
        }

        // Assert
        await _dbContextFactory.DidNotReceive().CreateDbContextAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ComposeSystemPrompt_WhenFullPersona_ShouldBuildCorrectString()
    {
        // Arrange
        var persona = new PersonaInput("TestBot", "A helpful bot", "Friendly", "In a chat");
        var systemPrompt = "You are an assistant.";

        // Act
        var result = GenerateCompletionHandler.ComposeSystemPrompt(persona, systemPrompt, null);

        // Assert
        result.ShouldContain("TestBot: A helpful bot");
        result.ShouldContain("Personality: Friendly");
        result.ShouldContain("Scenario: In a chat");
        result.ShouldContain(systemPrompt);
    }

    [Fact]
    public void ComposeSystemPrompt_WhenNoPersonalityOrScenario_ShouldOmitThem()
    {
        // Arrange
        var persona = new PersonaInput("TestBot", "A helpful bot");
        var systemPrompt = "You are an assistant.";

        // Act
        var result = GenerateCompletionHandler.ComposeSystemPrompt(persona, systemPrompt, null);

        // Assert
        result.ShouldContain("TestBot: A helpful bot");
        result.ShouldNotContain("Personality:");
        result.ShouldNotContain("Scenario:");
        result.ShouldContain(systemPrompt);
    }

    [Fact]
    public void ComposeSystemPrompt_WhenTraitsProvided_ShouldAppend()
    {
        // Arrange
        var persona = new PersonaInput("TestBot", "A helpful bot");
        var systemPrompt = "Base prompt.";
        List<TraitInput> traits = [new("Kindness", "Always be kind"), new("Brevity", "Be concise")];

        // Act
        var result = GenerateCompletionHandler.ComposeSystemPrompt(persona, systemPrompt, traits);

        // Assert
        result.ShouldContain("[Kindness]");
        result.ShouldContain("Always be kind");
        result.ShouldContain("[Brevity]");
        result.ShouldContain("Be concise");
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
            await foreach (var _ in _handler.Handle(command, cancellationToken))
            {
            }
        };

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void GenerateCompletionResponse_Content_ShouldBeAccessible()
    {
        // Arrange & Act
        var response = new GenerateCompletionResponse("Hello world");

        // Assert
        response.Content.ShouldBe("Hello world");
    }
}
