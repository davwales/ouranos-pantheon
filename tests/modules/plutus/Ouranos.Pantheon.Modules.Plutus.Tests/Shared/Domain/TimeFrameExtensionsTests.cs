using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain;

public sealed class TimeFrameExtensionsTests
{
    [Fact]
    public void ToTimeSpan_WhenGivenAllTime_ShouldReturnNull()
    {
        // Act
        var result = TimeFrame.AllTime.ToTimeSpan();

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(TimeFrame.FifteenMinutes, 15)]
    [InlineData(TimeFrame.OneHour, 60)]
    [InlineData(TimeFrame.FourHours, 240)]
    [InlineData(TimeFrame.OneDay, 1440)]
    [InlineData(TimeFrame.OneWeek, 10080)]
    [InlineData(TimeFrame.OneMonth, 43200)]
    [InlineData(TimeFrame.SixMonths, 262080)]
    [InlineData(TimeFrame.OneYear, 525600)]
    public void ToTimeSpan_ShouldReturnExpectedValue(TimeFrame frame, int expectedMinutes)
    {
        // Act
        var result = frame.ToTimeSpan();

        // Assert
        result.ShouldNotBeNull();
        result.Value.TotalMinutes.ShouldBe(expectedMinutes);
    }

    [Fact]
    public void ToTimeSpan_ShouldCoverAllEnumValues()
    {
        // Arrange
        var allValues = Enum.GetValues<TimeFrame>();

        // Act & Assert
        foreach (var value in allValues)
        {
            var action = () => value.ToTimeSpan();
            action.ShouldNotThrow();
        }
    }
}
