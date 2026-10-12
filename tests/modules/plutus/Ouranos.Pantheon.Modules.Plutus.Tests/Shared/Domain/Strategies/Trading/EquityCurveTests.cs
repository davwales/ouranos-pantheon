using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Trading;

public sealed class EquityCurveTests
{
    [Fact]
    public void Constructor_ShouldStartAtThePeakWithNoValuesOrDrawdown()
    {
        // Act
        var curve = new EquityCurve(1000m);

        // Assert
        curve.Peak.ShouldBe(1000m);
        curve.Values.ShouldBeEmpty();
        curve.MaxDrawdown.ShouldBe(0m);
    }

    [Fact]
    public void Record_ShouldAppendValuesAndTrackThePeak()
    {
        // Arrange
        var curve = new EquityCurve(1000m);

        // Act
        curve.Record(1200m);
        curve.Record(1100m);

        // Assert
        curve.Values.ShouldBe([1200m, 1100m]);
        curve.Peak.ShouldBe(1200m);
    }

    [Fact]
    public void Record_ShouldKeepTheLargestPeakToTroughFall()
    {
        // Arrange
        var curve = new EquityCurve(1000m);

        // Act
        curve.Record(800m);
        curve.Record(2000m);
        curve.Record(1800m);

        // Assert
        curve.MaxDrawdown.ShouldBe(0.2m);
    }

    [Fact]
    public void Record_WhenEquityNeverFallsBelowTheStart_ShouldHaveNoDrawdown()
    {
        // Arrange
        var curve = new EquityCurve(1000m);

        // Act
        curve.Record(1000m);
        curve.Record(1500m);

        // Assert
        curve.MaxDrawdown.ShouldBe(0m);
    }

    [Fact]
    public void Record_WhenPeakIsZero_ShouldNotDivideByZero()
    {
        // Arrange
        var curve = new EquityCurve(0m);

        // Act
        curve.Record(0m);

        // Assert
        curve.MaxDrawdown.ShouldBe(0m);
        curve.Peak.ShouldBe(0m);
    }
}
