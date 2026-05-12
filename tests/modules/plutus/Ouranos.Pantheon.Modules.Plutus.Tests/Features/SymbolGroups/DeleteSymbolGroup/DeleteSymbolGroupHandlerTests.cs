using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.DeleteSymbolGroup;

public sealed class DeleteSymbolGroupHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteSymbolGroupHandler _handler;
    private readonly ILogger<DeleteSymbolGroupHandler> _logger = Substitute.For<
        ILogger<DeleteSymbolGroupHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public DeleteSymbolGroupHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new DeleteSymbolGroupHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenSymbolGroupExists_ShouldDeleteAndReturnId()
    {
        // Arrange
        var group = SymbolGroup.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            new Id<Market>(Guid.NewGuid().ToString())
        );
        await _dbContext.SeedData(group);

        var command = new DeleteSymbolGroupInput(group.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<SymbolGroup>>();
        result.Id.ShouldBe(group.Id);

        var deleted = await _dbContext.SymbolGroups.FindAsync(group.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenSymbolGroupNotFound_ShouldReturnId()
    {
        // Arrange
        var id = new Id<SymbolGroup>(_fixture.Create<string>());
        var command = new DeleteSymbolGroupInput(id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<SymbolGroup>>();
        result.Id.ShouldBe(id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteSymbolGroupInput(new Id<SymbolGroup>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var delete = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await delete.ShouldThrowAsync<OperationCanceledException>();
    }
}
