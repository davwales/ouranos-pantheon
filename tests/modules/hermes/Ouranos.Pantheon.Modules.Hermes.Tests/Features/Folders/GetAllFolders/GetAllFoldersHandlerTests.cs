using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Folders.GetAllFolders;

public sealed class GetAllFoldersHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllFoldersHandler _handler;
    private readonly ILogger<GetAllFoldersHandler> _logger = Substitute.For<
        ILogger<GetAllFoldersHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetAllFoldersHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAllFoldersHandler(_logger, _dbContext, _flagsmith);

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
    public async Task Handle_WhenParentFolderIdIsNull_ShouldReturnAllFolders()
    {
        // Arrange
        var rootFolder = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Root Folder");
        var childFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Child Folder",
            parentFolderId: rootFolder.Id
        );
        await _dbContext.SeedData(rootFolder, childFolder);

        var query = new GetAllFoldersInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(f => f.Id == rootFolder.Id);
        result.ShouldContain(f => f.Id == childFolder.Id);
    }

    [Fact]
    public async Task Handle_WhenParentFolderIdSpecified_ShouldReturnChildFolders()
    {
        // Arrange
        var rootFolder = Folder.Create(DatabaseExtensions.CreateId<Folder>(), "Root");
        var child1 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Child 1",
            parentFolderId: rootFolder.Id
        );
        var child2 = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Child 2",
            parentFolderId: rootFolder.Id
        );
        await _dbContext.SeedData(rootFolder, child1, child2);

        var query = new GetAllFoldersInput(ParentFolderId: rootFolder.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(f => f.Id == child1.Id);
        result.ShouldContain(f => f.Id == child2.Id);
    }

    [Fact]
    public async Task Handle_WhenPublicModeEnabled_ShouldExcludeNonPublicFolders()
    {
        // Arrange
        SetupPublicMode(true);

        var publicFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Public",
            isPublic: true
        );
        var privateFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Private",
            isPublic: false
        );
        var extraFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Protected",
            isPublic: true
        );
        await _dbContext.SeedData(publicFolder, privateFolder, extraFolder);

        var query = new GetAllFoldersInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(f => f.Id == publicFolder.Id);
        result.ShouldContain(f => f.Id == extraFolder.Id);
        result.ShouldNotContain(f => f.Id == privateFolder.Id);
    }

    [Fact]
    public async Task Handle_WhenPublicModeDisabled_ShouldReturnAllFolders()
    {
        // Arrange
        SetupPublicMode(false);

        var publicFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Public",
            isPublic: true
        );
        var privateFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Private",
            isPublic: false
        );
        var extraFolder = Folder.Create(
            DatabaseExtensions.CreateId<Folder>(),
            "Protected",
            isPublic: true
        );
        await _dbContext.SeedData(publicFolder, privateFolder, extraFolder);

        var query = new GetAllFoldersInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_WhenNoFoldersExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllFoldersInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllFoldersInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
