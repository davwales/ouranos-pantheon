using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.UpdateConversation;

public sealed class UpdateConversationHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateConversationHandler _handler;
    private readonly ILogger<UpdateConversationHandler> _logger = Substitute.For<
        ILogger<UpdateConversationHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public UpdateConversationHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdateConversationHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateConversationAndReturnId()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(conversation);

        var newName = _fixture.Create<string>();
        var newPersonaId = new Id<Persona>(Guid.NewGuid().ToString());
        var newModelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var command = new UpdateConversationInput(
            conversation.Id,
            newName,
            newPersonaId,
            newModelConfigId,
            [],
            [],
            true
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Conversation>>();
        result.Id.ShouldBe(conversation.Id);

        var updated = await _dbContext.Conversations.FindAsync(conversation.Id);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe(newName);
        updated.PersonaId.ShouldBe(newPersonaId);
        updated.ModelConfigId.ShouldBe(newModelConfigId);
    }

    [Fact]
    public async Task Handle_WhenMessagesProvided_ShouldReplaceMessages()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(conversation);

        var messageInput = new UpdateConversationMessageInput("Hello World", Role.User);
        var command = new UpdateConversationInput(
            conversation.Id,
            _fixture.Create<string>(),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [messageInput],
            true
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Conversation>>();
        result.Id.ShouldBe(conversation.Id);
        messageInput.Content.ShouldBe("Hello World");
        messageInput.Role.ShouldBe(Role.User);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateConversationInput(
            new Id<Conversation>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            true
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateConversationInput(
            new Id<Conversation>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            true
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
