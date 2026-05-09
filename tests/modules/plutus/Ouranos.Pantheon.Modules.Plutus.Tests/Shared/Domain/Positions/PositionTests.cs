using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Positions;

public sealed class PositionTests
{
    private readonly IFixture _fixture;

    public PositionTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var side = _fixture.Create<PositionSide>();
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        // Act
        var position = Position.Create(side, marketId, symbolId, cost, quantity);

        // Assert
        position.Side.ShouldBe(side);
        position.MarketId.ShouldBe(marketId);
        position.SymbolId.ShouldBe(symbolId);
        position.Cost.ShouldBe(cost);
        position.Quantity.ShouldBe(quantity);
        position.Status.ShouldBe(PositionStatus.Pending);
        position.StrategyId.ShouldBeNull();
        position.Notes.ShouldBeNull();
        position.LinkedBuyPositionId.ShouldBeNull();
        position.Id.Value.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_WhenHappyPathWithOptionalStrategyIdAndNotes_ShouldSetOptionals()
    {
        // Arrange
        var side = _fixture.Create<PositionSide>();
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();
        var strategyId = _fixture.Create<Id<Strategy>>();
        var notes = _fixture.Create<string>();

        // Act
        var position = Position.Create(side, marketId, symbolId, cost, quantity, strategyId, notes);

        // Assert
        position.StrategyId.ShouldBe(strategyId);
        position.Notes.ShouldBe(notes);
    }

    [Fact]
    public void Create_WhenHappyPathWithLinkedBuyPosition_ShouldSetLinkedBuyPositionId()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var buyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);
        buyPosition.Close(PositionStatus.Bought);

        // Act
        var sellPosition = Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            cost,
            quantity,
            linkedBuyPosition: buyPosition
        );

        // Assert
        sellPosition.LinkedBuyPositionId.ShouldBe(buyPosition.Id);
    }

    [Fact]
    public void Create_WhenZeroCost_ShouldThrowArgumentException()
    {
        // Arrange
        var side = _fixture.Create<PositionSide>();
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var quantity = _fixture.Create<decimal>();

        // Act
        var create = () => Position.Create(side, marketId, symbolId, 0m, quantity);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenNegativeCost_ShouldThrowArgumentException()
    {
        // Arrange
        var side = _fixture.Create<PositionSide>();
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var quantity = _fixture.Create<decimal>();

        // Act
        var create = () => Position.Create(side, marketId, symbolId, -1m, quantity);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var side = _fixture.Create<PositionSide>();
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();

        // Act
        var create = () => Position.Create(side, marketId, symbolId, cost, 0m);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var side = _fixture.Create<PositionSide>();
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();

        // Act
        var create = () => Position.Create(side, marketId, symbolId, cost, -1m);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenLinkedBuyPositionIsNotBoughtBuy_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var pendingBuyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var create = () => Position.Create(
            PositionSide.Sell,
            marketId,
            symbolId,
            cost,
            quantity,
            linkedBuyPosition: pendingBuyPosition
        );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenSymbolDoesNotMatchNavProp_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var differentSymbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            differentSymbolId,
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            _fixture.Create<Id<Market>>(),
            _fixture.Create<AdditionalFields>()
        );

        // Act
        var create = () => Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            cost,
            quantity,
            symbol: symbol
        );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenHappyPathWithSymbolNavProp_ShouldSetSymbol()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var symbol = Symbol.Create(
            symbolId,
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            _fixture.Create<Id<Market>>(),
            _fixture.Create<AdditionalFields>()
        );

        // Act
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            cost,
            quantity,
            symbol: symbol
        );

        // Assert
        position.SymbolId.ShouldBe(symbolId);
        position.Symbol.ShouldBe(symbol);
    }

    [Fact]
    public void Symbol_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var access = () => position.Symbol;

        // Assert
        access.ShouldThrow<NavigationPropertyNotLoadedException<Position>>();
    }

    [Fact]
    public void Modify_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        var newCost = cost + 10m;
        var newQuantity = quantity + 5m;
        var newNotes = _fixture.Create<string>();

        // Act
        position.Modify(newCost, newQuantity, newNotes);

        // Assert
        position.Cost.ShouldBe(newCost);
        position.Quantity.ShouldBe(newQuantity);
        position.Notes.ShouldBe(newNotes);
    }

    [Fact]
    public void Modify_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);
        position.Close(PositionStatus.Bought);

        // Act
        var modify = () => position.Modify(cost, quantity, null);

        // Assert
        modify.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Modify_WhenZeroCost_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var modify = () => position.Modify(0m, quantity, null);

        // Assert
        modify.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Modify_WhenNegativeCost_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var modify = () => position.Modify(-1m, quantity, null);

        // Assert
        modify.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Modify_WhenZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var modify = () => position.Modify(cost, 0m, null);

        // Assert
        modify.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Modify_WhenNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var modify = () => position.Modify(cost, -1m, null);

        // Assert
        modify.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Close_WhenHappyPathBuyBought_ShouldSetStatusToBought()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        position.Close(PositionStatus.Bought);

        // Assert
        position.Status.ShouldBe(PositionStatus.Bought);
    }

    [Fact]
    public void Close_WhenHappyPathBuyDidNotBuy_ShouldSetStatusToDidNotBuy()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        position.Close(PositionStatus.DidNotBuy);

        // Assert
        position.Status.ShouldBe(PositionStatus.DidNotBuy);
    }

    [Fact]
    public void Close_WhenHappyPathSellSold_ShouldSetStatusToSold()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        position.Close(PositionStatus.Sold);

        // Assert
        position.Status.ShouldBe(PositionStatus.Sold);
    }

    [Fact]
    public void Close_WhenHappyPathSellDidNotSell_ShouldSetStatusToDidNotSell()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        position.Close(PositionStatus.DidNotSell);

        // Assert
        position.Status.ShouldBe(PositionStatus.DidNotSell);
    }

    [Fact]
    public void Close_WhenBuyPositionClosedWithSellStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var close = () => position.Close(PositionStatus.Sold);

        // Assert
        close.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Close_WhenBuyPositionClosedWithDidNotSellStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var close = () => position.Close(PositionStatus.DidNotSell);

        // Assert
        close.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Close_WhenSellPositionClosedWithBuyStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var close = () => position.Close(PositionStatus.Bought);

        // Assert
        close.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Close_WhenSellPositionClosedWithDidNotBuyStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var close = () => position.Close(PositionStatus.DidNotBuy);

        // Assert
        close.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);
        position.Close(PositionStatus.Bought);

        // Act
        var close = () => position.Close(PositionStatus.DidNotBuy);

        // Assert
        close.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void CanCloseWith_WhenBuyPositionWithBought_ShouldReturnTrue()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.Bought);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanCloseWith_WhenBuyPositionWithDidNotBuy_ShouldReturnTrue()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.DidNotBuy);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanCloseWith_WhenBuyPositionWithSold_ShouldReturnFalse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.Sold);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanCloseWith_WhenBuyPositionWithDidNotSell_ShouldReturnFalse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.DidNotSell);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanCloseWith_WhenSellPositionWithSold_ShouldReturnTrue()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.Sold);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanCloseWith_WhenSellPositionWithDidNotSell_ShouldReturnTrue()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.DidNotSell);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanCloseWith_WhenSellPositionWithBought_ShouldReturnFalse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.Bought);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanCloseWith_WhenSellPositionWithDidNotBuy_ShouldReturnFalse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanCloseWith(PositionStatus.DidNotBuy);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanBeLinkedAsTarget_WhenBoughtBuyPosition_ShouldReturnTrue()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);
        position.Close(PositionStatus.Bought);

        // Act
        var result = position.CanBeLinkedAsTarget();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanBeLinkedAsTarget_WhenPendingBuyPosition_ShouldReturnFalse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanBeLinkedAsTarget();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanBeLinkedAsTarget_WhenSellPosition_ShouldReturnFalse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var result = position.CanBeLinkedAsTarget();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void LinkPosition_WhenHappyPath_ShouldSetLinkedBuyPositionId()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var sellPosition = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);
        var buyPositionId = _fixture.Create<Id<Position>>();

        // Act
        sellPosition.LinkPosition(buyPositionId);

        // Assert
        sellPosition.LinkedBuyPositionId.ShouldBe(buyPositionId);
    }

    [Fact]
    public void LinkPosition_WhenBuyPosition_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var buyPosition = Position.Create(PositionSide.Buy, marketId, symbolId, cost, quantity);
        var buyPositionId = _fixture.Create<Id<Position>>();

        // Act
        var link = () => buyPosition.LinkPosition(buyPositionId);

        // Assert
        link.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void LinkPosition_WhenDefaultBuyPositionId_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var sellPosition = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var link = () => sellPosition.LinkPosition(default);

        // Assert
        link.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void LinkedBuyPosition_WhenNotLoadedAndIdIsNotNull_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var sellPosition = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);
        var buyPositionId = _fixture.Create<Id<Position>>();
        sellPosition.LinkPosition(buyPositionId);

        // Act
        var access = () => sellPosition.LinkedBuyPosition;

        // Assert
        access.ShouldThrow<NavigationPropertyNotLoadedException<Position>>();
    }

    [Fact]
    public void LinkedBuyPosition_WhenIdIsNull_ShouldReturnNull()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var cost = _fixture.Create<decimal>();
        var quantity = _fixture.Create<decimal>();

        var position = Position.Create(PositionSide.Sell, marketId, symbolId, cost, quantity);

        // Act
        var linkedBuyPosition = position.LinkedBuyPosition;

        // Assert
        linkedBuyPosition.ShouldBeNull();
    }
}
