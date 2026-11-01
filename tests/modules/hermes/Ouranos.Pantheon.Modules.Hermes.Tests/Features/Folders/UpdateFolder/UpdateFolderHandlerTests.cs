using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.UpdateFolder;

public sealed class UpdateFolderHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateFolderHandler _handler;
    private readonly ILogger<UpdateFolderHandler> _logger = Substitute.For<
        ILogger<UpdateFolderHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public UpdateFolderHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdateFolderHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateNameAndIsPublic()
    {
        // Arrange
        var folder = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Original Name");
        await _dbContext.SeedData(folder);

        var newName = "Updated Name";
        var command = new UpdateFolderInput(folder.Id, newName, IsPublic: false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Folder>>();
        result.Id.ShouldBe(folder.Id);

        var updated = await _dbContext.Folders.FindAsync(folder.Id);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe(newName);
        updated.IsPublic.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenSelfReferenceParent_ShouldThrowBadRequestException()
    {
        // Arrange
        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(folder);

        var command = new UpdateFolderInput(
            folder.Id,
            _fixture.Create<string>(),
            IsPublic: true,
            ParentFolderId: folder.Id
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenCycleDetected_ShouldThrowBadRequestException()
    {
        // Arrange
        var parent = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Parent");
        var child = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Child",
            parentFolderId: parent.Id
        );
        await _dbContext.SeedData(parent, child);

        var command = new UpdateFolderInput(
            parent.Id,
            _fixture.Create<string>(),
            IsPublic: true,
            ParentFolderId: child.Id
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenMaxDepthExceededByMoving_ShouldThrowBadRequestException()
    {
        // Arrange
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "root");
        await _dbContext.SeedData(root);

        var level1 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "level1",
            parentFolderId: root.Id
        );
        await _dbContext.SeedData(level1);

        var level2 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "level2",
            parentFolderId: level1.Id
        );
        await _dbContext.SeedData(level2);

        var level3 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "level3",
            parentFolderId: level2.Id
        );
        await _dbContext.SeedData(level3);

        var level4 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "level4",
            parentFolderId: level3.Id
        );
        await _dbContext.SeedData(level4);

        var movingFolder = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "mover");
        await _dbContext.SeedData(movingFolder);

        var command = new UpdateFolderInput(
            movingFolder.Id,
            _fixture.Create<string>(),
            IsPublic: true,
            ParentFolderId: level4.Id
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenMovedSubtreeWouldExceedMaxDepth_ShouldThrowBadRequestException()
    {
        // Arrange
        var target = await SeedChainAsync(Folder.MaxDepth - 2);
        var mover = await SeedChainAsync(3);

        var command = new UpdateFolderInput(
            mover[0].Id,
            _fixture.Create<string>(),
            IsPublic: true,
            ParentFolderId: target[^1].Id
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await handle.ShouldThrowAsync<BadRequestException>();
        exception.Message.ShouldContain(Folder.MaxDepth.ToString());
    }

    [Fact]
    public async Task Handle_WhenMovedSubtreeFitsExactlyAtMaxDepth_ShouldMoveFolder()
    {
        // Arrange
        var target = await SeedChainAsync(Folder.MaxDepth - 3);
        var mover = await SeedChainAsync(3);

        var command = new UpdateFolderInput(
            mover[0].Id,
            _fixture.Create<string>(),
            IsPublic: true,
            ParentFolderId: target[^1].Id
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var moved = await _dbContext.Folders.FindAsync(mover[0].Id);
        moved.ShouldNotBeNull();
        moved.ParentFolderId.ShouldBe(target[^1].Id);
    }

    [Fact]
    public async Task Handle_WhenMovedIntoGrandchild_ShouldThrowBadRequestException()
    {
        // Arrange
        var chain = await SeedChainAsync(3);

        var command = new UpdateFolderInput(
            chain[0].Id,
            _fixture.Create<string>(),
            IsPublic: true,
            ParentFolderId: chain[2].Id
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenParentUnchangedInOverDeepTree_ShouldStillRename()
    {
        // Arrange
        var chain = await SeedChainAsync(Folder.MaxDepth + 1);

        var command = new UpdateFolderInput(
            chain[1].Id,
            "Renamed",
            IsPublic: true,
            ParentFolderId: chain[0].Id
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Folders.FindAsync(chain[1].Id);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe("Renamed");
    }

    [Fact]
    public async Task Handle_WhenRenamingWithoutChangingParent_ShouldUpdateNameOnly()
    {
        // Arrange
        var parentId = DatabaseExtensions.CreateId<Folder>();
        var parent = Folder.Create(parentId, "Parent");
        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Original Name",
            parentFolderId: parentId
        );
        await _dbContext.SeedData(parent, folder);

        var command = new UpdateFolderInput(
            folder.Id,
            "New Name",
            IsPublic: true,
            ParentFolderId: parentId
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Folder>>();
        var updated = await _dbContext.Folders.FindAsync(folder.Id);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe("New Name");
        updated.ParentFolderId.ShouldBe(parentId);
    }

    [Fact]
    public async Task Handle_WhenFolderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateFolderInput(
            new Id<Folder>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            IsPublic: true
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
        var command = new UpdateFolderInput(
            new Id<Folder>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            IsPublic: true
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<List<Folder>> SeedChainAsync(int length)
    {
        var chain = new List<Folder>();
        Id<Folder>? parentId = null;

        for (var i = 0; i < length; i++)
        {
            var folder = Folder.Create(
                DatabaseExtensions.CreateId<Folder>(),
                _fixture.Create<string>(),
                parentFolderId: parentId
            );
            await _dbContext.SeedData(folder);
            chain.Add(folder);
            parentId = folder.Id;
        }

        return chain;
    }
}
