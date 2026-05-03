using Ouranos.Pantheon.Modules.Plutus.Features.Trades.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.Shared;

public sealed class SmartIntervalCalculatorTests
{
    [Fact]
    public void Calculate_WhenDurationIsZero_ShouldReturnFiveMinuteDefault()
    {
        // Act
        var result = SmartIntervalCalculator.Calculate(TimeSpan.Zero, 100);

        // Assert
        result.ShouldBe(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Calculate_WhenDurationIsNegative_ShouldReturnFiveMinuteDefault()
    {
        // Act
        var result = SmartIntervalCalculator.Calculate(TimeSpan.FromSeconds(-1), 100);

        // Assert
        result.ShouldBe(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Calculate_WhenTargetBucketSizeIsUnderSixtySeconds_ShouldNotReturnOneSecond()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(30);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 10);

        // Assert
        result.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Calculate_WhenTargetIsUnderSixtySeconds_ShouldEnforceMinimumOfTenSeconds()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 100);

        // Assert
        result.ShouldBe(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Calculate_WhenTargetIsExactlyFortySeconds_ShouldRoundToFortySeconds()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(400);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 10);

        // Assert
        result.ShouldBe(TimeSpan.FromSeconds(40));
    }

    [Fact]
    public void Calculate_WhenTargetIsInMinuteRange_ShouldReturnWholeMinutes()
    {
        // Arrange
        var duration = TimeSpan.FromHours(1);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 10);

        // Assert
        result.ShouldBe(TimeSpan.FromMinutes(6));
    }

    [Fact]
    public void Calculate_WhenTargetIsInMinuteRange_ShouldEnforceMinimumOfOneMinute()
    {
        // Arrange
        var duration = TimeSpan.FromHours(2);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 100);

        // Assert
        result.ShouldBe(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Calculate_WhenTargetIsInHourRange_ShouldReturnHourlyInterval()
    {
        // Arrange
        var duration = TimeSpan.FromDays(1);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 10);

        // Assert
        result.ShouldBe(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Calculate_WhenTargetIsInDayRange_ShouldReturnDailyInterval()
    {
        // Arrange
        var duration = TimeSpan.FromDays(30);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 10);

        // Assert
        result.ShouldBe(TimeSpan.FromDays(3));
    }

    [Fact]
    public void Calculate_WhenTargetIsInYearRange_ShouldReturnDays()
    {
        // Arrange
        var duration = TimeSpan.FromDays(365);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 10);

        // Assert
        result.ShouldBe(TimeSpan.FromDays(36));
    }

    [Fact]
    public void Calculate_WhenTargetIsMultiYear_ShouldReturnWeekMultiples()
    {
        // Arrange
        var duration = TimeSpan.FromDays(4000);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 10);

        // Assert
        (result.TotalDays % 7).ShouldBe(0);
        result.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromDays(365));
    }

    [Fact]
    public void Calculate_WhenTargetIsVeryLarge_ShouldEnforceMinimumOfSevenDays()
    {
        // Arrange
        var duration = TimeSpan.FromDays(3650);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 100);

        // Assert
        result.TotalDays.ShouldBeGreaterThanOrEqualTo(7);
    }

    [Fact]
    public void Calculate_WhenNumBucketsIsOne_ShouldReturnIntervalCoveringFullDuration()
    {
        // Arrange
        var duration = TimeSpan.FromHours(1);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 1);

        // Assert
        result.ShouldBe(TimeSpan.FromMinutes(60));
    }

    [Fact]
    public void Calculate_WhenDurationExactlyOneHour_ShouldReturnSixtyMinuteInterval()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(3600);

        // Act
        var result = SmartIntervalCalculator.Calculate(duration, 1);

        // Assert
        result.ShouldBe(TimeSpan.FromMinutes(60));
    }
}
