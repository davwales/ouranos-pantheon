using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.LinkPosition;

public sealed class LinkPositionHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly LinkPositionHandler _handler;

    private readonly ILogger<LinkPositionHandler> _logger = Substitute.For<
        ILogger<LinkPositionHandler>
    >();

    private readonly PlutusDbContext _dbContext;

    public LinkPositionHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new LinkPositionHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldLinkPosition()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var buyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, 150.50m, 10m);
        buyPosition.Close(PositionStatus.Bought);

        var sellPosition = Position.Create(PositionSide.Sell, marketId, symbolId, 155.00m, 10m);

        await _dbContext.Positions.AddAsync(buyPosition);
        await _dbContext.Positions.AddAsync(sellPosition);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(sellPosition.Id, buyPosition.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Position>>();
        result.Id.ShouldBe(sellPosition.Id);

        var saved = await _dbContext.Positions.FindAsync(sellPosition.Id);
        saved.ShouldNotBeNull();
        saved.LinkedBuyPositionId.ShouldBe(buyPosition.Id);
    }

    [Fact]
    public async Task Handle_WhenSellPositionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new LinkPositionInput(
            _fixture.Create<Id<Position>>(),
            _fixture.Create<Id<Position>>()
        );

        // Act
        var link = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await link.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBuyPositionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var sellPosition = Position.Create(PositionSide.Sell, marketId, symbolId, 155.00m, 10m);
        await _dbContext.Positions.AddAsync(sellPosition);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(sellPosition.Id, _fixture.Create<Id<Position>>());

        // Act
        var link = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await link.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLinkingBuyPositionToBuyPosition_ShouldThrowBadRequestException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId1 = _fixture.Create<Id<Symbol>>();
        var symbolId2 = _fixture.Create<Id<Symbol>>();
        var buyPosition1 = Position.Create(PositionSide.Buy, marketId, symbolId1, 150.50m, 10m);
        var buyPosition2 = Position.Create(PositionSide.Buy, marketId, symbolId2, 200.00m, 5m);
        await _dbContext.Positions.AddAsync(buyPosition1);
        await _dbContext.Positions.AddAsync(buyPosition2);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(buyPosition1.Id, buyPosition2.Id);

        // Act
        var link = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await link.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenLinkingToSellPosition_ShouldThrowBadRequestException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId1 = _fixture.Create<Id<Symbol>>();
        var symbolId2 = _fixture.Create<Id<Symbol>>();
        var sellPosition1 = Position.Create(PositionSide.Sell, marketId, symbolId1, 155.00m, 10m);
        var sellPosition2 = Position.Create(PositionSide.Sell, marketId, symbolId2, 200.00m, 5m);
        await _dbContext.Positions.AddAsync(sellPosition1);
        await _dbContext.Positions.AddAsync(sellPosition2);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(sellPosition1.Id, sellPosition2.Id);

        // Act
        var link = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await link.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenBuyPositionIsForAnotherSymbol_ShouldThrowBadRequestException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var buyPosition = Position.Create(
            PositionSide.Buy,
            marketId,
            _fixture.Create<Id<Symbol>>(),
            150.50m,
            10m
        );
        buyPosition.Close(PositionStatus.Bought);
        var sellPosition = Position.Create(
            PositionSide.Sell,
            marketId,
            _fixture.Create<Id<Symbol>>(),
            155.00m,
            10m
        );
        await _dbContext.Positions.AddRangeAsync(buyPosition, sellPosition);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(sellPosition.Id, buyPosition.Id);

        // Act
        var link = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await link.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenOtherSellsCoverTheBuy_ShouldThrowBadRequestException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var buyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, 150.50m, 10m);
        buyPosition.Close(PositionStatus.Bought);
        var soldPosition = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            155.00m,
            8m,
            linkedBuyPosition: buyPosition
        );
        var sellPosition = Position.Create(PositionSide.Sell, marketId, symbolId, 155.00m, 3m);
        await _dbContext.Positions.AddRangeAsync(buyPosition, soldPosition, sellPosition);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(sellPosition.Id, buyPosition.Id);

        // Act
        var link = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await link.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenOtherLinkedSellDidNotSell_ShouldLinkPosition()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var buyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, 150.50m, 10m);
        buyPosition.Close(PositionStatus.Bought);
        var unsoldPosition = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            155.00m,
            10m,
            linkedBuyPosition: buyPosition
        );
        unsoldPosition.Close(PositionStatus.DidNotSell);
        var sellPosition = Position.Create(PositionSide.Sell, marketId, symbolId, 152.00m, 10m);
        await _dbContext.Positions.AddRangeAsync(buyPosition, unsoldPosition, sellPosition);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(sellPosition.Id, buyPosition.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await _dbContext.Positions.FindAsync(sellPosition.Id);
        saved.ShouldNotBeNull();
        saved.LinkedBuyPositionId.ShouldBe(buyPosition.Id);
    }

    [Fact]
    public async Task Handle_WhenRelinkingSameSell_ShouldNotCountItsOwnQuantity()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var buyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, 150.50m, 10m);
        buyPosition.Close(PositionStatus.Bought);
        var sellPosition = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            155.00m,
            10m,
            linkedBuyPosition: buyPosition
        );
        await _dbContext.Positions.AddRangeAsync(buyPosition, sellPosition);
        await _dbContext.SaveChangesAsync();

        var command = new LinkPositionInput(sellPosition.Id, buyPosition.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(sellPosition.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new LinkPositionInput(
            _fixture.Create<Id<Position>>(),
            _fixture.Create<Id<Position>>()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var link = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await link.ShouldThrowAsync<OperationCanceledException>();
    }
}
