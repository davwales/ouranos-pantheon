using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.UpdateSymbolGroup;

public sealed class UpdateSymbolGroupHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateSymbolGroupHandler _handler;
    private readonly ILogger<UpdateSymbolGroupHandler> _logger = Substitute.For<
        ILogger<UpdateSymbolGroupHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public UpdateSymbolGroupHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new UpdateSymbolGroupHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var group = SymbolGroup.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            "Original Name",
            "Original Description",
            new Id<Market>(Guid.NewGuid().ToString())
        );

        var command = new UpdateSymbolGroupInput(
            group.Id,
            "Updated Name",
            "Updated Description",
            []
        );

        await _dbContext.SeedData(group);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<SymbolGroup>>();
        result.Id.ShouldBe(group.Id);

        var updated = await _dbContext.SymbolGroups.FindAsync(group.Id);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe("Updated Name");
        updated.Description.ShouldBe("Updated Description");
    }

    [Fact]
    public async Task Handle_WhenMembersChanged_ShouldAddAndRemoveMembers()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol1 = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "SYM1",
            null,
            "Symbol 1",
            market.Id,
            new AdditionalFields()
        );
        var symbol2 = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "SYM2",
            null,
            "Symbol 2",
            market.Id,
            new AdditionalFields()
        );
        var symbol3 = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "SYM3",
            null,
            "Symbol 3",
            market.Id,
            new AdditionalFields()
        );

        var group = SymbolGroup.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            market.Id
        );

        var existingMember = SymbolGroupMember.Create(group.Id, symbol1.Id);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol1, symbol2, symbol3);
        await _dbContext.SeedData(group);
        await _dbContext.SeedData(existingMember);

        var command = new UpdateSymbolGroupInput(
            group.Id,
            _fixture.Create<string>(),
            null,
            [symbol1.Id, symbol2.Id]
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var members = _dbContext
            .SymbolGroupMembers.Where(m => m.SymbolGroupId == group.Id)
            .Select(m => m.SymbolId)
            .ToList();

        members.Count.ShouldBe(2);
        members.ShouldContain(symbol1.Id);
        members.ShouldContain(symbol2.Id);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateSymbolGroupInput(
            new Id<SymbolGroup>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            []
        );

        // Act
        var update = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateSymbolGroupInput(
            new Id<SymbolGroup>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            []
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
