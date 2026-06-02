using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations.Schemas;
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

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GetAllConversations;

public sealed class GetAllConversationsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllConversationsHandler _handler;

    private readonly ILogger<GetAllConversationsHandler> _logger = Substitute.For<
        ILogger<GetAllConversationsHandler>
    >();

    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetAllConversationsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAllConversationsHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    private static Conversation CreateConversation(bool isPublic)
    {
        return Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            Guid.NewGuid().ToString(),
            isPublic
        );
    }

    [Fact]
    public async Task Handle_WhenConversationsExist_ShouldReturnAll()
    {
        // Arrange
        var conv1 = CreateConversation(true);
        var conv2 = CreateConversation(false);
        await _dbContext.SeedData(conv1, conv2);

        var query = new GetAllConversationsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<List<GetAllConversationsResponse>>();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenNoConversations_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllConversationsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPublicModeEnabled_ShouldOnlyReturnPublicConversations()
    {
        // Arrange
        var publicConv = CreateConversation(true);
        var privateConv = CreateConversation(false);
        await _dbContext.SeedData(publicConv, privateConv);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetAllConversationsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        var item = result[0];
        item.Id.ShouldBe(publicConv.Id);
        item.Name.ShouldNotBeNull();
        item.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllConversationsInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenFolderIdProvided_WithUnprotectedFolder_ShouldReturnConversations()
    {
        // Arrange
        var folder = Folder.Create(new Id<Folder>(Guid.NewGuid().ToString()), "Unprotected Folder");
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            Guid.NewGuid().ToString(),
            true,
            folderId: folder.Id
        );
        await _dbContext.SeedData(folder);
        await _dbContext.SeedData(conversation);

        var query = new GetAllConversationsInput(FolderId: folder.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(conversation.Id);
    }

    [Fact]
    public async Task Handle_WhenConversationsExist_ShouldReturnResponseWithAllProperties()
    {
        // Arrange
        var conv = CreateConversation(true);
        await _dbContext.SeedData(conv);

        var query = new GetAllConversationsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        var item = result[0];
        item.CreatedAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        item.UpdatedAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
    }
}
