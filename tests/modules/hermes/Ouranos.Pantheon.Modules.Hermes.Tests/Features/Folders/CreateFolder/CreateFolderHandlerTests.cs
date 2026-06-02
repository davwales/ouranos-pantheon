using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.CreateFolder;

public sealed class CreateFolderHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateFolderHandler _handler;
    private readonly ILogger<CreateFolderHandler> _logger = Substitute.For<
        ILogger<CreateFolderHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public CreateFolderHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new CreateFolderHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateRootFolderAndReturnResponse()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var command = new CreateFolderInput(name);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreateFolderResponse>();
        result.FolderId.ShouldNotBe(default);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldPersistFolderToDatabase()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var command = new CreateFolderInput(name);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedFolder = await _dbContext.Folders.FindAsync(result.FolderId);
        savedFolder.ShouldNotBeNull();
        savedFolder.Name.ShouldBe(name);
        savedFolder.IsPublic.ShouldBeTrue();
        savedFolder.ParentFolderId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenParentFolderIdProvidedAndValid_ShouldCreateChildFolder()
    {
        // Arrange
        var parentFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(parentFolder);

        var name = _fixture.Create<string>();
        var command = new CreateFolderInput(name, ParentFolderId: parentFolder.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedFolder = await _dbContext.Folders.FindAsync(result.FolderId);
        savedFolder.ShouldNotBeNull();
        savedFolder.ParentFolderId.ShouldBe(parentFolder.Id);
    }

    [Fact]
    public async Task Handle_WhenParentFolderNotFound_ShouldThrowArgumentException()
    {
        // Arrange
        var parentId = DatabaseExtensions.CreateId<Folder>();
        var command = new CreateFolderInput(_fixture.Create<string>(), ParentFolderId: parentId);

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenMaxDepthExceeded_ShouldThrowArgumentException()
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

        var level5 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "level5",
            parentFolderId: level4.Id
        );
        await _dbContext.SeedData(level5);

        var command = new CreateFolderInput(_fixture.Create<string>(), ParentFolderId: level5.Id);

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenParentIsPublic_ShouldKeepChildIsPublicAsSpecified()
    {
        // Arrange
        var parentFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            _fixture.Create<string>(),
            isPublic: true
        );
        await _dbContext.SeedData(parentFolder);

        var name = _fixture.Create<string>();
        var command = new CreateFolderInput(name, ParentFolderId: parentFolder.Id, IsPublic: true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedFolder = await _dbContext.Folders.FindAsync(result.FolderId);
        savedFolder.ShouldNotBeNull();
        savedFolder.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateFolderInput(_fixture.Create<string>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
