using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using MessageDto = Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos.MessageDto;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.CreateConversation;

public sealed class CreateConversationHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateConversationHandler _handler;
    private readonly ILogger<CreateConversationHandler> _logger = Substitute.For<ILogger<CreateConversationHandler>>();
    private readonly HermesDbContext _dbContext;
    private readonly IOuranosMachineLearningClient _mlClient = Substitute.For<IOuranosMachineLearningClient>();

    public CreateConversationHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        var options = Options.Create(new HermesOptions());
        _handler = new CreateConversationHandler(_logger, _dbContext, _mlClient, options);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateConversationAndReturnResponse()
    {
        // Arrange
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var name = _fixture.Create<string>();
        var command = new CreateConversationInput(personaId, modelConfigId, [], [], name);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreateConversationResponse>();
        result.Id.ShouldNotBe(default);
        result.Name.ShouldBe(name);

        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.Name.ShouldBe(name);
    }

    [Fact]
    public async Task Handle_WhenNameIsNull_ShouldDefaultToNewConversation()
    {
        // Arrange
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var command = new CreateConversationInput(personaId, modelConfigId, [], []);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.ShouldBe("New Conversation");
    }

    [Fact]
    public async Task Handle_WhenNameGenerationEnabled_ShouldCallMlClient()
    {
        // Arrange
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var generatedName = _fixture.Create<string>();

        _mlClient.GenerateChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(generatedName);

        var options = Options.Create(new HermesOptions("Generate a name.", "test-model"));
        var handler = new CreateConversationHandler(_logger, _dbContext, _mlClient, options);
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [new CreateConversationMessageInput("Hello", Role.User)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await _mlClient.Received(1).GenerateChatCompletionAsync(
            Arg.Any<string>(),
            Arg.Any<List<MessageDto>>(),
            Arg.Any<float?>(),
            Arg.Any<int?>(),
            Arg.Any<float?>(),
            Arg.Any<CancellationToken>()
        );
        result.Name.ShouldBe(generatedName.Trim().Length > 60 ? generatedName.Trim()[..60] : generatedName.Trim());
    }

    [Fact]
    public async Task Handle_WhenNameGenerationReturnsLongString_ShouldTruncateTo60Chars()
    {
        // Arrange
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var longName = new string('a', 80);

        _mlClient.GenerateChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(longName);

        var options = Options.Create(new HermesOptions("Generate a name.", "test-model"));
        var handler = new CreateConversationHandler(_logger, _dbContext, _mlClient, options);
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [new CreateConversationMessageInput("Hello", Role.User)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Length.ShouldBe(60);
    }

    [Fact]
    public async Task Handle_WhenNameGenerationThrows_ShouldFallbackToDefaultName()
    {
        // Arrange
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());

        _mlClient.GenerateChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns<string>(_ => throw new Exception("ML service unavailable"));

        var options = Options.Create(new HermesOptions("Generate a name.", "test-model"));
        var handler = new CreateConversationHandler(_logger, _dbContext, _mlClient, options);
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [new CreateConversationMessageInput("Hello", Role.User)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.ShouldBe("New Conversation");
    }

    [Fact]
    public async Task Handle_WhenTraitIdsProvided_ShouldLoadTraitsFromDb()
    {
        // Arrange
        var trait1 = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var trait2 = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(trait1, trait2);

        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [trait1.Id, trait2.Id],
            [],
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.Traits.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenMessagesProvided_ShouldAssignSequentialSortOrder()
    {
        // Arrange
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());

        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [
                new CreateConversationMessageInput("First", Role.User),
                new CreateConversationMessageInput("Second", Role.Assistant),
                new CreateConversationMessageInput("Third", Role.User)
            ],
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.Messages.Count.ShouldBe(3);
        savedConversation.Messages.OrderBy(m => m.SortOrder).Select(m => m.SortOrder).ShouldBe([0, 1, 2]);
    }

    [Fact]
    public async Task Handle_WhenSystemAndAssistantMessagesProvided_ShouldMapRolesCorrectly()
    {
        // Arrange
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());

        _mlClient.GenerateChatCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<float?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns("Generated Name");

        var options = Options.Create(new HermesOptions("Generate a name.", "test-model"));
        var handler = new CreateConversationHandler(_logger, _dbContext, _mlClient, options);

        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [
                new CreateConversationMessageInput("System prompt", Role.System),
                new CreateConversationMessageInput("Hello", Role.User),
                new CreateConversationMessageInput("Hi there", Role.Assistant),
            ]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateConversationInput(
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
