using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.CreatePosition;

public sealed class CreatePositionHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreatePositionHandler _handler;
    private readonly ILogger<CreatePositionHandler> _logger = Substitute.For<
        ILogger<CreatePositionHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public CreatePositionHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new CreatePositionHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreatePositionAndReturnId()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var command = new CreatePositionInput(PositionSide.Buy, marketId, symbol.Id, 150.50m, 10m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Position>>();
        result.Id.ShouldNotBe(default);

        var position = await _dbContext.Positions.FindAsync(result.Id);
        position.ShouldNotBeNull();
        position.Side.ShouldBe(PositionSide.Buy);
        position.SymbolId.ShouldBe(symbol.Id);
        position.Cost.ShouldBe(150.50m);
        position.Quantity.ShouldBe(10m);
        position.MarketId.ShouldBe(marketId);
        position.Status.ShouldBe(PositionStatus.Pending);
    }

    [Fact]
    public async Task Handle_WhenTargetPriceProvided_ShouldPersistTargetPrice()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var command = new CreatePositionInput(
            PositionSide.Buy,
            marketId,
            symbol.Id,
            150.50m,
            10m,
            TargetPrice: 180m
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var position = await _dbContext.Positions.FindAsync(result.Id);
        position.ShouldNotBeNull();
        position.TargetPrice.ShouldBe(180m);
    }

    [Fact]
    public async Task Handle_WhenCreatingSellPosition_ShouldCreateSellWithNoLink()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var command = new CreatePositionInput(PositionSide.Sell, marketId, symbol.Id, 155.00m, 10m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Position>>();

        var position = await _dbContext.Positions.FindAsync(result.Id);
        position.ShouldNotBeNull();
        position.Side.ShouldBe(PositionSide.Sell);
        position.LinkedBuyPositionId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenStrategyInMarket_ShouldPersistStrategyId()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var strategy = await AddStrategyAsync(marketId);
        var command = new CreatePositionInput(
            PositionSide.Buy,
            marketId,
            symbol.Id,
            150.50m,
            10m,
            strategy.Id
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var position = await _dbContext.Positions.FindAsync(result.Id);
        position.ShouldNotBeNull();
        position.StrategyId.ShouldBe(strategy.Id);
    }

    [Fact]
    public async Task Handle_WhenSymbolInAnotherMarket_ShouldThrowBadRequestException()
    {
        // Arrange
        var symbol = await AddSymbolAsync(_fixture.Create<Id<Market>>());
        var command = new CreatePositionInput(
            PositionSide.Buy,
            _fixture.Create<Id<Market>>(),
            symbol.Id,
            150.50m,
            10m
        );

        // Act
        var create = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await create.ShouldThrowAsync<BadRequestException>();
        _dbContext.Positions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSymbolDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreatePositionInput(
            PositionSide.Buy,
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            150.50m,
            10m
        );

        // Act
        var create = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await create.ShouldThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenStrategyInAnotherMarket_ShouldThrowBadRequestException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var strategy = await AddStrategyAsync(_fixture.Create<Id<Market>>());
        var command = new CreatePositionInput(
            PositionSide.Buy,
            marketId,
            symbol.Id,
            150.50m,
            10m,
            strategy.Id
        );

        // Act
        var create = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await create.ShouldThrowAsync<BadRequestException>();
        _dbContext.Positions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenLinkedBuyPositionGiven_ShouldCreateLinkedSell()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var buyPosition = await AddBoughtBuyAsync(marketId, symbol.Id, 10m);
        var command = new CreatePositionInput(
            PositionSide.Sell,
            marketId,
            symbol.Id,
            155.00m,
            10m,
            LinkedBuyPositionId: buyPosition.Id
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var position = await _dbContext.Positions.FindAsync(result.Id);
        position.ShouldNotBeNull();
        position.LinkedBuyPositionId.ShouldBe(buyPosition.Id);
        _dbContext.Positions.Count().ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenLinkedBuyPositionDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var command = new CreatePositionInput(
            PositionSide.Sell,
            marketId,
            symbol.Id,
            155.00m,
            10m,
            LinkedBuyPositionId: _fixture.Create<Id<Position>>()
        );

        // Act
        var create = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await create.ShouldThrowAsync<BadRequestException>();
        _dbContext.Positions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenLinkedBuyAlreadySold_ShouldThrowBadRequestAndCreateNothing()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbol = await AddSymbolAsync(marketId);
        var buyPosition = await AddBoughtBuyAsync(marketId, symbol.Id, 10m);
        var soldPosition = Position.Create(
            PositionSide.Sell,
            marketId,
            symbol.Id,
            155.00m,
            10m,
            linkedBuyPosition: buyPosition
        );
        await _dbContext.Positions.AddAsync(soldPosition);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePositionInput(
            PositionSide.Sell,
            marketId,
            symbol.Id,
            155.00m,
            1m,
            LinkedBuyPositionId: buyPosition.Id
        );

        // Act
        var create = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await create.ShouldThrowAsync<BadRequestException>();
        _dbContext.Positions.Count().ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreatePositionInput(
            PositionSide.Buy,
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            150.50m,
            10m
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<Symbol> AddSymbolAsync(Id<Market> marketId)
    {
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.SaveChangesAsync();
        return symbol;
    }

    private async Task<Position> AddBoughtBuyAsync(
        Id<Market> marketId,
        Id<Symbol> symbolId,
        decimal quantity
    )
    {
        var buyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, 150m, quantity);
        buyPosition.Close(PositionStatus.Bought);
        await _dbContext.Positions.AddAsync(buyPosition);
        await _dbContext.SaveChangesAsync();
        return buyPosition;
    }

    private async Task<Strategy> AddStrategyAsync(Id<Market> marketId)
    {
        var strategy = Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();
        return strategy;
    }
}
