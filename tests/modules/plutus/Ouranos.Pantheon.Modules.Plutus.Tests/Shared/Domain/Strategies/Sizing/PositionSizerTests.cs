using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Sizing;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Sizing;

public sealed class PositionSizerTests
{
    [Fact]
    public void Size_WhenUnconstrained_ShouldSpendPositionBudget()
    {
        // Act
        var size = PositionSizer.Size(10m, 1000m, 1_000_000m, null, 2m, 0.1m);

        // Assert
        size.ShouldBe(new PositionSize(100m, PositionLimit.Budget));
    }

    [Fact]
    public void Size_WhenFillCapIsBinding_ShouldBeLimitedByLiquidity()
    {
        // Act
        var size = PositionSizer.Size(10m, 1000m, 40m, null, 2m, 0.1m);

        // Assert
        size.ShouldBe(new PositionSize(40m, PositionLimit.Liquidity));
    }

    [Fact]
    public void Size_WhenBuyLimitIsBinding_ShouldUseLimitTimesWindows()
    {
        // Act
        var size = PositionSizer.Size(10m, 1000m, 1_000_000m, 15m, 2m, 0.1m);

        // Assert
        size.ShouldBe(new PositionSize(30m, PositionLimit.BuyLimit));
    }

    [Fact]
    public void Size_WhenCappedValueBelowMinFillFraction_ShouldReturnZeroVolume()
    {
        // Act
        var size = PositionSizer.Size(10m, 1000m, 9m, null, 2m, 0.1m);

        // Assert
        size.Volume.ShouldBe(0m);
        size.LimitedBy.ShouldBe(PositionLimit.Liquidity);
    }

    [Theory]
    [InlineData(0, 1000)]
    [InlineData(10, 0)]
    public void Size_WhenPriceOrBudgetNotPositive_ShouldReturnZeroVolume(
        int price,
        int positionBudget
    )
    {
        // Act
        var size = PositionSizer.Size(price, positionBudget, 1000m, null, 2m, 0.1m);

        // Assert
        size.Volume.ShouldBe(0m);
    }
}
