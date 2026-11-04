using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Folders;

public sealed class FolderVisibilityTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();

    private static Folder CreateFolder(bool isPublic = true, Id<Folder>? parentFolderId = null)
    {
        return Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            Guid.NewGuid().ToString(),
            isPublic,
            parentFolderId
        );
    }

    [Fact]
    public async Task GetHiddenFolderIdsAsync_WhenAllPublic_ShouldReturnEmpty()
    {
        // Arrange
        var root = CreateFolder();
        var child = CreateFolder(parentFolderId: root.Id);
        await _dbContext.SeedData(root, child);

        // Act
        var result = await _dbContext.GetHiddenFolderIdsAsync(CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetHiddenFolderIdsAsync_WhenFolderPrivate_ShouldHideItsSubtreeOnly()
    {
        // Arrange
        var root = CreateFolder();
        var privateFolder = CreateFolder(isPublic: false, parentFolderId: root.Id);
        var child = CreateFolder(parentFolderId: privateFolder.Id);
        var grandchild = CreateFolder(parentFolderId: child.Id);
        var sibling = CreateFolder(parentFolderId: root.Id);
        await _dbContext.SeedData(grandchild, child, root, privateFolder, sibling);

        // Act
        var result = await _dbContext.GetHiddenFolderIdsAsync(CancellationToken.None);

        // Assert
        result.ShouldBe([privateFolder.Id, child.Id, grandchild.Id], ignoreOrder: true);
    }

    [Fact]
    public async Task GetHiddenFolderIdsAsync_WhenParentsFormACycle_ShouldStillReturn()
    {
        // Arrange
        var first = DatabaseExtensions.CreateId<Folder>();
        var second = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Second",
            parentFolderId: first
        );
        await _dbContext.SeedData(Folder.Create(first, "First", parentFolderId: second.Id), second);

        // Act
        var result = await _dbContext.GetHiddenFolderIdsAsync(CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task IsFolderHiddenAsync_WhenFolderIdIsNull_ShouldReturnFalse()
    {
        // Act
        var result = await _dbContext.IsFolderHiddenAsync(null, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsFolderHiddenAsync_WhenAncestorPrivate_ShouldReturnTrue()
    {
        // Arrange
        var grandparent = CreateFolder(isPublic: false);
        var parent = CreateFolder(parentFolderId: grandparent.Id);
        var folder = CreateFolder(parentFolderId: parent.Id);
        await _dbContext.SeedData(grandparent, parent, folder);

        // Act
        var result = await _dbContext.IsFolderHiddenAsync(folder.Id, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsFolderHiddenAsync_WhenChainPublic_ShouldReturnFalse()
    {
        // Arrange
        var parent = CreateFolder();
        var folder = CreateFolder(parentFolderId: parent.Id);
        await _dbContext.SeedData(parent, folder);

        // Act
        var result = await _dbContext.IsFolderHiddenAsync(folder.Id, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsFolderHiddenAsync_WhenFolderMissing_ShouldReturnFalse()
    {
        // Act
        var result = await _dbContext.IsFolderHiddenAsync(
            DatabaseExtensions.CreateId<Folder>(),
            CancellationToken.None
        );

        // Assert
        result.ShouldBeFalse();
    }
}
