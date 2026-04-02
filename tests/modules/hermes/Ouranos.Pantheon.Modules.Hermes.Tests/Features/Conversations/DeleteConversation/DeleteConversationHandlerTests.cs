using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.DeleteConversation;

public sealed class DeleteConversationHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteConversationHandler _handler;
    private readonly ILogger<DeleteConversationHandler> _logger = Substitute.For<ILogger<DeleteConversationHandler>>();
    private readonly HermesDbContext _dbContext;

    public DeleteConversationHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new DeleteConversationHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteConversationAndReturnId()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [], [],
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(conversation);

        var command = new DeleteConversationInput(conversation.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Conversation>>();
        result.Id.ShouldBe(conversation.Id);

        var deleted = await _dbContext.Conversations.FindAsync(conversation.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteConversationInput(new Id<Conversation>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteConversationInput(new Id<Conversation>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
