using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.DeleteFolder;

public sealed class DeleteFolderHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteFolderHandler _handler;
    private readonly ILogger<DeleteFolderHandler> _logger = Substitute.For<
        ILogger<DeleteFolderHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public DeleteFolderHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new DeleteFolderHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteFolderAndReturnId()
    {
        // Arrange
        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(folder);

        var command = new DeleteFolderInput(folder.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Folder>>();
        result.Id.ShouldBe(folder.Id);

        var deleted = await _dbContext.Folders.FindAsync(folder.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenFolderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteFolderInput(new Id<Folder>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenFolderHasChildren_ShouldCascadeDeleteChildren()
    {
        // Arrange
        var parent = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Parent");
        var child = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Child",
            parentFolderId: parent.Id
        );
        var grandchild = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Grandchild",
            parentFolderId: child.Id
        );
        await _dbContext.SeedData(parent, child, grandchild);

        var command = new DeleteFolderInput(parent.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(parent.Id);

        var deletedParent = await _dbContext.Folders.FindAsync(parent.Id);
        deletedParent.ShouldBeNull();

        var deletedChild = await _dbContext.Folders.FindAsync(child.Id);
        deletedChild.ShouldBeNull();

        var deletedGrandchild = await _dbContext.Folders.FindAsync(grandchild.Id);
        deletedGrandchild.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteFolderInput(new Id<Folder>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
