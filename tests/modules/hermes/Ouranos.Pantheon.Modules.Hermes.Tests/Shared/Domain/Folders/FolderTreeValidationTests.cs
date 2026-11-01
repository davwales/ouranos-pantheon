using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Folders;

public sealed class FolderTreeValidationTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly HermesDbContext _dbContext;

    public FolderTreeValidationTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
    }

    [Fact]
    public async Task CheckForLoopAsync_WhenTargetIsAncestor_ShouldReturnTrue()
    {
        // Arrange
        var grandparent = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        var parent = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: grandparent.Id
        );
        var child = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: parent.Id
        );
        await _dbContext.SeedData(grandparent, parent, child);

        // Act
        var result = await FolderTreeValidation.CheckForLoopAsync(
            child.Id,
            _dbContext,
            targetFolderId: grandparent.Id,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task CheckForLoopAsync_WhenTargetIsNotAncestor_ShouldReturnFalse()
    {
        // Arrange
        var folderA = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        var folderB = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(folderA, folderB);

        // Act
        var result = await FolderTreeValidation.CheckForLoopAsync(
            folderB.Id,
            _dbContext,
            targetFolderId: folderA.Id,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckForLoopAsync_WhenNoTargetAndNoParent_ShouldReturnFalse()
    {
        // Arrange
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), _fixture.Create<string>());
        await _dbContext.SeedData(root);

        // Act
        var result = await FolderTreeValidation.CheckForLoopAsync(
            root.Id,
            _dbContext,
            cancellationToken: CancellationToken.None
        );

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckForLoopAsync_WhenFolderDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = DatabaseExtensions.CreateId<Folder>();

        // Act
        var result = await FolderTreeValidation.CheckForLoopAsync(
            nonExistentId,
            _dbContext,
            cancellationToken: CancellationToken.None
        );

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckForLoopAsync_WhenDeepChainHasLoop_ShouldReturnTrue()
    {
        // Arrange: root -> a -> b -> c -> d
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), _fixture.Create<string>());
        var a = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: root.Id
        );
        var b = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: a.Id
        );
        var c = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: b.Id
        );
        var d = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: c.Id
        );
        await _dbContext.SeedData(root, a, b, c, d);

        // Act
        var result = await FolderTreeValidation.CheckForLoopAsync(
            d.Id,
            _dbContext,
            targetFolderId: root.Id,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task CalculateDepthAsync_WhenRootFolder_ShouldReturnOne()
    {
        // Arrange
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), _fixture.Create<string>());
        await _dbContext.SeedData(root);

        // Act
        var result = await FolderTreeValidation.CalculateDepthAsync(
            root.Id,
            _dbContext,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task CalculateDepthAsync_WhenDepthThree_ShouldReturnThree()
    {
        // Arrange
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), _fixture.Create<string>());
        var child = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: root.Id
        );
        var grandchild = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: child.Id
        );
        await _dbContext.SeedData(root, child, grandchild);

        // Act
        var result = await FolderTreeValidation.CalculateDepthAsync(
            grandchild.Id,
            _dbContext,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(3);
    }

    [Fact]
    public async Task CalculateDepthAsync_WhenMaxDepth_ShouldReturnMaxDepth()
    {
        // Arrange
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), _fixture.Create<string>());
        await _dbContext.SeedData(root);

        var current = root;
        for (var i = 1; i < Folder.MaxDepth; i++)
        {
            var next = Folder.Create(
                DatabaseExtensions.CreateId<Folder>(),
                _fixture.Create<string>(),
                parentFolderId: current.Id
            );
            await _dbContext.SeedData(next);
            current = next;
        }

        // Act
        var result = await FolderTreeValidation.CalculateDepthAsync(
            current.Id,
            _dbContext,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(Folder.MaxDepth);
    }

    [Fact]
    public async Task CalculateDepthAsync_WhenFolderDoesNotExist_ShouldReturnOne()
    {
        // Arrange
        var nonExistentId = DatabaseExtensions.CreateId<Folder>();

        // Act
        var result = await FolderTreeValidation.CalculateDepthAsync(
            nonExistentId,
            _dbContext,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task CalculateSubtreeHeightAsync_WhenNoSubfolders_ShouldReturnOne()
    {
        // Arrange
        var folder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(folder);

        // Act
        var result = await FolderTreeValidation.CalculateSubtreeHeightAsync(
            folder.Id,
            _dbContext,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task CalculateSubtreeHeightAsync_WhenBranchesDiffer_ShouldReturnDeepestBranch()
    {
        // Arrange: root -> (a -> a1 -> a2), (b)
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), _fixture.Create<string>());
        var a = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: root.Id
        );
        var b = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: root.Id
        );
        var a1 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: a.Id
        );
        var a2 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            parentFolderId: a1.Id
        );
        await _dbContext.SeedData(root, a, b, a1, a2);

        // Act
        var result = await FolderTreeValidation.CalculateSubtreeHeightAsync(
            root.Id,
            _dbContext,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(4);
    }

    [Fact]
    public async Task CalculateSubtreeHeightAsync_WhenDeeperThanMaxDepth_ShouldStopPastMaxDepth()
    {
        // Arrange
        var root = Folder.Create(DatabaseExtensions.CreateId<Folder>(), _fixture.Create<string>());
        await _dbContext.SeedData(root);

        var current = root;
        for (var i = 0; i < Folder.MaxDepth + 3; i++)
        {
            var next = Folder.Create(
                DatabaseExtensions.CreateId<Folder>(),
                _fixture.Create<string>(),
                parentFolderId: current.Id
            );
            await _dbContext.SeedData(next);
            current = next;
        }

        // Act
        var result = await FolderTreeValidation.CalculateSubtreeHeightAsync(
            root.Id,
            _dbContext,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(Folder.MaxDepth + 1);
    }
}
