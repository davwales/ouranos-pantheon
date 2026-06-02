using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;
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
