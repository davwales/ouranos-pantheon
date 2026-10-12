using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Sizing;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Sizing;

public sealed class LiquidityMathTests
{
    [Fact]
    public void MedianDailyVolume_WhenFewDaysTraded_ShouldCountMissingDaysAsZero()
    {
        // Act
        var median = LiquidityMath.MedianDailyVolume([500m, 1000m, 2000m], 14);

        // Assert
        median.ShouldBe(0m);
    }

    [Fact]
    public void MedianDailyVolume_WhenOddWindow_ShouldReturnMiddleValue()
    {
        // Act
        var median = LiquidityMath.MedianDailyVolume([5m, 1m, 3m], 3);

        // Assert
        median.ShouldBe(3m);
    }

    [Fact]
    public void MedianDailyVolume_WhenEvenWindow_ShouldAverageMiddleValues()
    {
        // Act
        var median = LiquidityMath.MedianDailyVolume([1m, 2m, 3m, 4m], 4);

        // Assert
        median.ShouldBe(2.5m);
    }

    [Theory]
    [InlineData(1000, 200, 50)]
    [InlineData(100, 200, 25)]
    [InlineData(0, 200, 0)]
    public void FillCap_ShouldUseSmallerOfDayAndTypicalVolume(
        int dayVolume,
        int typicalVolume,
        int expected
    )
    {
        // Act
        var cap = LiquidityMath.FillCap(dayVolume, typicalVolume, 0.25m);

        // Assert
        cap.ShouldBe(expected);
    }
}
