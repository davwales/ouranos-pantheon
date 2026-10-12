using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization;

public sealed class ParameterSpaceTests
{
    private static readonly Parameter[] Parameters =
    [
        new("lookbackHours", 4, 72, IsWhole: true),
        new("bidOffset", -0.3, 0.4),
    ];

    private static StrategyConfiguration Build(IReadOnlyList<double> genes)
    {
        return new FlipConfiguration(
            LookbackHours: (int)genes[0],
            BidOffset: ParameterSpace.ToDecimal(genes[1])
        );
    }

    [Fact]
    public void Constructor_ShouldClampTheSeedIntoEachParametersBounds()
    {
        // Act
        var space = new ParameterSpace(Parameters, [100.7, -1], Build);

        // Assert
        space.Parameters.ShouldBe(Parameters);
        space.Seed.ShouldBe([72d, -0.3d]);
    }

    [Fact]
    public void Constructor_WhenSeedLengthDiffers_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new ParameterSpace(Parameters, [1], Build));
    }

    [Fact]
    public void Constructor_WhenBuildIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new ParameterSpace(Parameters, [10, 0], null!));
    }

    [Fact]
    public void Build_ShouldCreateTheConfigurationFromTheValues()
    {
        // Arrange
        var space = new ParameterSpace(Parameters, [10, 0], Build);

        // Act
        var configuration = space.Build([12, 0.25]);

        // Assert
        configuration.ShouldBe(new FlipConfiguration(LookbackHours: 12, BidOffset: 0.25m));
    }

    [Fact]
    public void Build_WhenValueCountDiffers_ShouldThrowArgumentException()
    {
        // Arrange
        var space = new ParameterSpace(Parameters, [10, 0], Build);

        // Act & Assert
        Should.Throw<ArgumentException>(() => space.Build([12]));
    }

    [Theory]
    [InlineData(0.1234567, 0.123457)]
    [InlineData(-0.3, -0.3)]
    [InlineData(2, 2)]
    public void ToDecimal_ShouldRoundToSixDecimalPlaces(double value, double expected)
    {
        // Act
        var result = ParameterSpace.ToDecimal(value);

        // Assert
        result.ShouldBe((decimal)expected);
    }
}
