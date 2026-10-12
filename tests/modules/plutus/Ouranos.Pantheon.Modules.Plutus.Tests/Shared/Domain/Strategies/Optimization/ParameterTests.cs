using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization;

public sealed class ParameterTests
{
    [Theory]
    [InlineData(-5, 0)]
    [InlineData(0.25, 0.25)]
    [InlineData(5, 1)]
    public void Clamp_ShouldKeepValuesWithinBounds(double value, double expected)
    {
        // Arrange
        var parameter = new Parameter("ratio", 0, 1);

        // Act
        var clamped = parameter.Clamp(value);

        // Assert
        clamped.ShouldBe(expected);
    }

    [Theory]
    [InlineData(3.4, 3)]
    [InlineData(3.6, 4)]
    [InlineData(99, 10)]
    public void Clamp_WhenWhole_ShouldRoundToTheNearestWholeNumber(double value, double expected)
    {
        // Arrange
        var parameter = new Parameter("count", 1, 10, IsWhole: true);

        // Act
        var clamped = parameter.Clamp(value);

        // Assert
        clamped.ShouldBe(expected);
    }
}
