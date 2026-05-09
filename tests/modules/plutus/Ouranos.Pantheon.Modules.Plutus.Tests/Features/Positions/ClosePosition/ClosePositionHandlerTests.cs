using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.ClosePosition;

public sealed class ClosePositionHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ClosePositionHandler _handler;
    private readonly ILogger<ClosePositionHandler> _logger = Substitute.For<ILogger<ClosePositionHandler>>();
    private readonly PlutusDbContext _dbContext;

    public ClosePositionHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new ClosePositionHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenClosingBuyAsBought_ShouldClosePositionAndReturnStatus()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m
        );
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new ClosePositionInput(position.Id, PositionStatus.Bought);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ClosePositionResponse>();
        result.PositionId.ShouldBe(position.Id);
        result.Status.ShouldBe(PositionStatus.Bought);

        var saved = await _dbContext.Positions.FindAsync(position.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(PositionStatus.Bought);
    }

    [Fact]
    public async Task Handle_WhenClosingBuyAsDidNotBuy_ShouldClosePositionAndReturnStatus()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m
        );
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new ClosePositionInput(position.Id, PositionStatus.DidNotBuy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(PositionStatus.DidNotBuy);

        var saved = await _dbContext.Positions.FindAsync(position.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(PositionStatus.DidNotBuy);
    }

    [Fact]
    public async Task Handle_WhenClosingSellAsSold_ShouldClosePositionAndReturnStatus()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            155.00m,
            10m
        );
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new ClosePositionInput(position.Id, PositionStatus.Sold);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(PositionStatus.Sold);

        var saved = await _dbContext.Positions.FindAsync(position.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(PositionStatus.Sold);
    }

    [Fact]
    public async Task Handle_WhenClosingSellAsDidNotSell_ShouldClosePositionAndReturnStatus()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            155.00m,
            10m
        );
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new ClosePositionInput(position.Id, PositionStatus.DidNotSell);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(PositionStatus.DidNotSell);

        var saved = await _dbContext.Positions.FindAsync(position.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(PositionStatus.DidNotSell);
    }

    [Fact]
    public async Task Handle_WhenClosingBuyWithSellStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m
        );
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new ClosePositionInput(position.Id, PositionStatus.DidNotSell);

        // Act
        var close = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await close.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenClosingSellWithBuyStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            155.00m,
            10m
        );
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new ClosePositionInput(position.Id, PositionStatus.Bought);

        // Act
        var close = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await close.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenPositionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new ClosePositionInput(
            _fixture.Create<Id<Position>>(),
            PositionStatus.Bought
        );

        // Act
        var close = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await close.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPositionAlreadyClosed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m
        );
        position.Close(PositionStatus.Bought);
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new ClosePositionInput(position.Id, PositionStatus.Bought);

        // Act
        var close = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await close.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new ClosePositionInput(
            _fixture.Create<Id<Position>>(),
            PositionStatus.Bought
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var close = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await close.ShouldThrowAsync<OperationCanceledException>();
    }
}
