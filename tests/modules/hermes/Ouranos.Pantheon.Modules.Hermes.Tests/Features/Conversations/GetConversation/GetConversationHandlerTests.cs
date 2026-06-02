using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;
using HermesTrait = Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits.Trait;
using HermesTraitId = Ouranos.Pantheon.Modules.Shared.Domain.Id<Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits.Trait>;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GetConversation;

public sealed class GetConversationHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetConversationHandler _handler;
    private readonly ILogger<GetConversationHandler> _logger = Substitute.For<
        ILogger<GetConversationHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetConversationHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetConversationHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnFullConversationResponse()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var conversationName = _fixture.Create<string>();
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            persona.Id,
            modelConfig.Id,
            [],
            [],
            conversationName
        );

        await _dbContext.SeedData(persona);
        await _dbContext.SeedData(modelConfig);
        await _dbContext.SeedData(conversation);

        var query = new GetConversationInput(conversation.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetConversationResponse>();
        result.Id.ShouldBe(conversation.Id);
        result.Name.ShouldBe(conversationName);
        result.IsPublic.ShouldBeTrue();

        result.Persona.Id.ShouldBe(persona.Id);
        result.Persona.Name.ShouldNotBeNull();
        result.Persona.Description.ShouldNotBeNull();

        result.Model.Id.ShouldBe(modelConfig.Id);
        result.Model.Name.ShouldNotBeNull();
        result.Model.ModelIdentifier.ShouldNotBeNull();
        result.Model.SystemPrompt.ShouldNotBeNull();

        result.Traits.ShouldBeEmpty();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenConversationHasTraits_ShouldReturnTraits()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var trait = HermesTrait.Create(
            new HermesTraitId(Guid.NewGuid().ToString()),
            "Bold",
            "Be bold and direct"
        );
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var conversation = Conversation.Create(
            conversationId,
            persona.Id,
            modelConfig.Id,
            [],
            [trait],
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(persona);
        await _dbContext.SeedData(modelConfig);
        await _dbContext.SeedData(conversation);

        var query = new GetConversationInput(conversationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Traits.Count.ShouldBe(1);
        var t = result.Traits[0];
        t.Id.ShouldBe(trait.Id);
        t.Name.ShouldBe("Bold");
        t.Content.ShouldBe("Be bold and direct");
    }

    [Fact]
    public async Task Handle_WhenConversationHasMessages_ShouldReturnMessages()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var message = Message.Create(
            new Id<Message>(Guid.NewGuid().ToString()),
            conversationId,
            "Hello",
            Role.User,
            0
        );
        var conversation = Conversation.Create(
            conversationId,
            persona.Id,
            modelConfig.Id,
            [message],
            [],
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(persona);
        await _dbContext.SeedData(modelConfig);
        await _dbContext.SeedData(conversation);

        var query = new GetConversationInput(conversationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Messages.ShouldNotBeEmpty();
        var msg = result.Messages.First();
        msg.Content.ShouldBe("Hello");
        msg.Id.ShouldNotBe(default);
        msg.Role.ShouldBe(Role.User);
        msg.SortOrder.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetConversationInput(new Id<Conversation>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeEnabled_ShouldThrowNotFoundException()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            persona.Id,
            modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            false
        );

        await _dbContext.SeedData(persona);
        await _dbContext.SeedData(modelConfig);
        await _dbContext.SeedData(conversation);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetConversationInput(conversation.Id);

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetConversationInput(new Id<Conversation>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenConversationInFolder_ShouldSucceed()
    {
        // Arrange
        var folder = Folder.Create(new Id<Folder>(Guid.NewGuid().ToString()), "Unprotected Folder");
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            persona.Id,
            modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            folderId: folder.Id
        );

        await _dbContext.SeedData(folder);
        await _dbContext.SeedData(persona);
        await _dbContext.SeedData(modelConfig);
        await _dbContext.SeedData(conversation);

        var query = new GetConversationInput(conversation.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetConversationResponse>();
        result.Id.ShouldBe(conversation.Id);
    }

    [Fact]
    public async Task Handle_WhenConversationHasAllFields_ShouldReturnResponseWithAllProperties()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            personality: "Friendly",
            scenario: "In a chat"
        );
        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            temperature: 0.7f,
            maxTokens: 512,
            repeatPenalty: 1.1f
        );
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            persona.Id,
            modelConfig.Id,
            [],
            [],
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(persona);
        await _dbContext.SeedData(modelConfig);
        await _dbContext.SeedData(conversation);

        var query = new GetConversationInput(conversation.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.CreatedAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        result.UpdatedAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        result.Persona.Personality.ShouldBe("Friendly");
        result.Persona.Scenario.ShouldBe("In a chat");
        result.Model.Temperature.ShouldBe(0.7f);
        result.Model.MaxTokens.ShouldBe(512);
        result.Model.RepeatPenalty.ShouldBe(1.1f);
    }
}
