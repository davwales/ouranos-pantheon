using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.GetFolder;

public sealed class GetFolderHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetFolderHandler _handler;
    private readonly ILogger<GetFolderHandler> _logger = Substitute.For<
        ILogger<GetFolderHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetFolderHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetFolderHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    private IFlags SetupPublicMode(bool enabled)
    {
        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(enabled));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
        return flags;
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnFolderResponse()
    {
        // Arrange
        SetupPublicMode(false);

        var folder = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Test Folder");
        await _dbContext.SeedData(folder);

        var query = new GetFolderInput(folder.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetFolderResponse>();
        result.Id.ShouldBe(folder.Id);
        result.Name.ShouldBe("Test Folder");
        result.IsPublic.ShouldBeTrue();
        result.ParentFolderId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenFolderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetFolderInput(new Id<Folder>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPrivateFolderInPublicMode_ShouldThrowNotFoundException()
    {
        // Arrange
        SetupPublicMode(true);

        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Private Folder",
            isPublic: false
        );
        await _dbContext.SeedData(folder);

        var query = new GetFolderInput(folder.Id);

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPublicFolderInPublicMode_ShouldReturnFolder()
    {
        // Arrange
        SetupPublicMode(true);

        var folder = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Open Folder");
        await _dbContext.SeedData(folder);

        var query = new GetFolderInput(folder.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(folder.Id);
        result.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenAncestorPrivateInPublicMode_ShouldThrowNotFoundException()
    {
        // Arrange
        SetupPublicMode(true);

        var grandparent = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Private",
            isPublic: false
        );
        var parent = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Public",
            parentFolderId: grandparent.Id
        );
        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Public Child",
            parentFolderId: parent.Id
        );
        await _dbContext.SeedData(grandparent, parent, folder);

        var query = new GetFolderInput(folder.Id);

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    public async Task Handle_ShouldCountOnlyVisibleChildrenInPublicMode(
        bool isPublicMode,
        int expectedCount
    )
    {
        // Arrange
        SetupPublicMode(isPublicMode);

        var folder = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Parent");
        var publicChild = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Public",
            parentFolderId: folder.Id
        );
        var privateChild = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Private",
            isPublic: false,
            parentFolderId: folder.Id
        );
        await _dbContext.SeedData(folder, publicChild, privateChild);
        await _dbContext.SeedData(
            CreateConversation(isPublic: true, folder.Id),
            CreateConversation(isPublic: false, folder.Id)
        );

        var query = new GetFolderInput(folder.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ConversationCount.ShouldBe(expectedCount);
        result.SubfolderCount.ShouldBe(expectedCount);
    }

    private static Conversation CreateConversation(bool isPublic, Id<Folder> folderId)
    {
        return Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            Guid.NewGuid().ToString(),
            isPublic,
            folderId: folderId
        );
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetFolderInput(new Id<Folder>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
