using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.GetAllPositions;

public sealed class GetAllPositionsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllPositionsHandler _handler;
    private readonly ILogger<GetAllPositionsHandler> _logger = Substitute.For<ILogger<GetAllPositionsHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetAllPositionsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllPositionsHandler(_logger, _dbContext, Options.Create(new QueryOptions()));
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnPagedPositions()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m,
            symbol: symbol
        );
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllPositionsInput(marketId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldNotBeEmpty();
        result.TotalCount.ShouldBe(1);
        result.Items.First().SymbolId.ShouldBe(symbolId);
        result.Items.First().SymbolName.ShouldBe("Test Symbol");
        result.Items.First().Side.ShouldBe(PositionSide.Buy);
    }

    [Fact]
    public async Task Handle_WhenFilteredBySide_ShouldReturnOnlyMatchingPositions()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var buyPosition = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m,
            symbol: symbol
        );
        var sellPosition = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            155.00m,
            10m,
            symbol: symbol
        );
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.Positions.AddAsync(buyPosition);
        await _dbContext.Positions.AddAsync(sellPosition);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllPositionsInput(marketId, Side: PositionSide.Buy);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldNotBeEmpty();
        result.TotalCount.ShouldBe(1);
        result.Items.All(p => p.Side == PositionSide.Buy).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenFilteredByStatus_ShouldReturnOnlyMatchingPositions()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId1 = _fixture.Create<Id<Symbol>>();
        var symbolId2 = _fixture.Create<Id<Symbol>>();
        var symbol1 = Symbol.Create(
            symbolId1,
            "SYM1",
            null,
            "Symbol One",
            marketId,
            new AdditionalFields()
        );
        var symbol2 = Symbol.Create(
            symbolId2,
            "SYM2",
            null,
            "Symbol Two",
            marketId,
            new AdditionalFields()
        );
        var pendingPosition = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId1,
            150.50m,
            10m,
            symbol: symbol1
        );
        var boughtPosition = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId2,
            200.00m,
            5m,
            symbol: symbol2
        );
        boughtPosition.Close(PositionStatus.Bought);

        await _dbContext.Symbols.AddAsync(symbol1);
        await _dbContext.Symbols.AddAsync(symbol2);
        await _dbContext.Positions.AddAsync(pendingPosition);
        await _dbContext.Positions.AddAsync(boughtPosition);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllPositionsInput(marketId, Status: PositionStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldNotBeEmpty();
        result.TotalCount.ShouldBe(1);
        result.Items.All(p => p.Status == PositionStatus.Pending).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllPositionsInput(_fixture.Create<Id<Market>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
