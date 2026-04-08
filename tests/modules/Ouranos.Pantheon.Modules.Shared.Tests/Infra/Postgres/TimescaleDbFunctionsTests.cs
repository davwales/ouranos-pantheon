using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Functions;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Postgres;

public sealed class TimescaleDbFunctionsTests
{
    [Fact]
    public void TimeBucket_DateTime_ShouldBucketToInterval()
    {
        // Arrange
        var interval = TimeSpan.FromHours(1);
        var timestamp = new DateTime(2026, 1, 1, 14, 35, 22, DateTimeKind.Utc);

        // Act
        var result = TimescaleDbFunctions.TimeBucket(interval, timestamp);

        // Assert
        result.Hour.ShouldBe(14);
        result.Minute.ShouldBe(0);
        result.Second.ShouldBe(0);
    }

    [Fact]
    public void TimeBucket_DateTimeOffset_ShouldBucketToInterval()
    {
        // Arrange
        var interval = TimeSpan.FromMinutes(15);
        var timestamp = new DateTimeOffset(2026, 1, 1, 14, 23, 45, TimeSpan.Zero);

        // Act
        var result = TimescaleDbFunctions.TimeBucket(interval, timestamp);

        // Assert
        result.Minute.ShouldBe(15);
        result.Second.ShouldBe(0);
    }

    [Fact]
    public void ToTimescaleInterval_WhenDays_ShouldReturnDaysString()
    {
        // Act
        var result = TimeSpan.FromDays(7).ToTimescaleInterval();

        // Assert
        result.ShouldBe("7 days");
    }

    [Fact]
    public void ToTimescaleInterval_WhenHours_ShouldReturnHoursString()
    {
        // Act
        var result = TimeSpan.FromHours(4).ToTimescaleInterval();

        // Assert
        result.ShouldBe("4 hours");
    }

    [Fact]
    public void ToTimescaleInterval_WhenMinutes_ShouldReturnMinutesString()
    {
        // Act
        var result = TimeSpan.FromMinutes(15).ToTimescaleInterval();

        // Assert
        result.ShouldBe("15 minutes");
    }

    [Fact]
    public void ToTimescaleInterval_WhenSeconds_ShouldReturnSecondsString()
    {
        // Act
        var result = TimeSpan.FromSeconds(30).ToTimescaleInterval();

        // Assert
        result.ShouldBe("30 seconds");
    }

    [Fact]
    public void HasTimescaleDbFunctions_ShouldRegisterBothOverloads()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();

        // Act
        var act = () => modelBuilder.HasTimescaleDbFunctions();

        // Assert
        act.ShouldNotThrow();
    }
}
