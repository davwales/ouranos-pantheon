using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Trading;

public sealed class HoldingTests
{
    private static readonly DateTimeOffset OpenedAt = new(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly Holding _holding = new(
        new Id<Symbol>(Guid.NewGuid().ToString()),
        "Item",
        OpenedAt
    );

    [Fact]
    public void Buy_ShouldAccumulateQuantityAndCostBasis()
    {
        // Act
        _holding.Buy(10m, 100m);
        _holding.Buy(30m, 120m);

        // Assert
        _holding.Quantity.ShouldBe(40m);
        _holding.Cost.ShouldBe(4600m);
        _holding.UnitCost.ShouldBe(115m);
        _holding.BoughtQuantity.ShouldBe(40m);
        _holding.BoughtCost.ShouldBe(4600m);
    }

    [Fact]
    public void UnitCost_WhenNothingIsHeld_ShouldBeZero()
    {
        // Act
        var unitCost = _holding.UnitCost;

        // Assert
        unitCost.ShouldBe(0m);
    }

    [Fact]
    public void Sell_WhenUnitsRemain_ShouldReduceCostAtUnitCostAndReturnNull()
    {
        // Arrange
        _holding.Buy(10m, 100m);

        // Act
        var closed = _holding.Sell(4m, 110m, 430m, OpenedAt.AddDays(1));

        // Assert
        closed.ShouldBeNull();
        _holding.Quantity.ShouldBe(6m);
        _holding.Cost.ShouldBe(600m);
        _holding.UnitCost.ShouldBe(100m);
        _holding.SoldGross.ShouldBe(440m);
        _holding.SoldNet.ShouldBe(430m);
    }

    [Fact]
    public void Sell_WhenTheLastUnitIsSold_ShouldReturnTheWholeRoundTripAsOnePosition()
    {
        // Arrange
        var exitTime = OpenedAt.AddDays(3);
        _holding.Buy(10m, 100m);
        _holding.Sell(4m, 110m, 440m, OpenedAt.AddDays(1));

        // Act
        var closed = _holding.Sell(6m, 120m, 700m, exitTime);

        // Assert
        closed.ShouldNotBeNull();
        closed.SymbolId.ShouldBe(_holding.SymbolId.Value);
        closed.SymbolName.ShouldBe("Item");
        closed.EntryPrice.ShouldBe(100m);
        closed.ExitPrice.ShouldBe(116m);
        closed.Volume.ShouldBe(10m);
        closed.ProfitLoss.ShouldBe(140m);
        closed.ReturnPercent.ShouldBe(0.14m);
        closed.EntryTime.ShouldBe(OpenedAt);
        closed.ExitTime.ShouldBe(exitTime);
        _holding.Quantity.ShouldBe(0m);
    }

    [Fact]
    public void Sell_WhenSoldAtALossAfterTax_ShouldReportANegativeReturn()
    {
        // Arrange
        _holding.Buy(10m, 100m);

        // Act
        var closed = _holding.Sell(10m, 90m, 880m, OpenedAt.AddDays(1));

        // Assert
        closed.ShouldNotBeNull();
        closed.ProfitLoss.ShouldBe(-120m);
        closed.ReturnPercent.ShouldBe(-0.12m);
    }

    [Fact]
    public void Sell_WhenTopUpFollowsPartialSale_ShouldAverageEveryBuyIntoTheEntryPrice()
    {
        // Arrange
        _holding.Buy(10m, 100m);
        _holding.Sell(5m, 110m, 550m, OpenedAt.AddHours(1));
        _holding.Buy(10m, 130m);

        // Act
        var closed = _holding.Sell(15m, 140m, 2100m, OpenedAt.AddHours(2));

        // Assert
        closed.ShouldNotBeNull();
        closed.Volume.ShouldBe(20m);
        closed.EntryPrice.ShouldBe(115m);
        closed.ProfitLoss.ShouldBe(350m);
    }
}
