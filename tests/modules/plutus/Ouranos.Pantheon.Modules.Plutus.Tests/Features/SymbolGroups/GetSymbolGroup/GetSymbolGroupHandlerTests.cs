using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.GetSymbolGroup;

public sealed class GetSymbolGroupHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetSymbolGroupHandler _handler;
    private readonly ILogger<GetSymbolGroupHandler> _logger = Substitute.For<ILogger<GetSymbolGroupHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetSymbolGroupHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetSymbolGroupHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnSymbolGroupResponse()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var group = SymbolGroup.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            market.Id
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(group);

        var query = new GetSymbolGroupInput(group.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(group.Id);
        result.MarketId.ShouldBe(group.MarketId);
        result.Name.ShouldBe(group.Name);
        result.Symbols.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenGroupHasMemberWithSnapshot_ShouldReturnMemberWithStats()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            new Taxes(null)
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var member = SymbolGroupMember.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            symbol.Id,
            symbol
        );

        var group = SymbolGroup.Create(
            member.SymbolGroupId,
            _fixture.Create<string>(),
            null,
            market.Id,
            members: [member]
        );

        var snapshot = MarketTradeSnapshot.Create(
            market.Id,
            symbol.Id,
            TimeFrame.OneDay,
            totalSpent: 10000m,
            minPrice: 90m,
            maxPrice: 110m,
            totalVolume: 100m,
            numTransactions: 50,
            limit: 200m,
            tax: 1m
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(group);
        await _dbContext.SeedData(snapshot);

        var query = new GetSymbolGroupInput(group.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Symbols.Count.ShouldBe(1);
        var sym = result.Symbols[0];
        sym.SymbolId.ShouldBe(symbol.Id);
        sym.Volume.ShouldNotBeNull();
        sym.Code.ShouldNotBeNull();
        sym.Name.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetSymbolGroupInput(new Id<SymbolGroup>(_fixture.Create<string>()));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetSymbolGroupInput(new Id<SymbolGroup>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
