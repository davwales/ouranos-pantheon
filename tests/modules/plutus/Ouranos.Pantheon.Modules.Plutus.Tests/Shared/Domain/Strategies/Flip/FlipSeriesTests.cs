using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Flip;

public sealed class FlipSeriesTests
{
    [Fact]
    public void Window_ShouldReturnVolumeWeightedSidePricesOverWholeHours()
    {
        // Arrange
        var series = FlipTestData.Series(
            3,
            i =>
                i < FlipMarketData.BarsPerHour
                    ? new FlipBar(110f, 10f, 100f, 30f)
                    : new FlipBar(120f, 30f, 90f, 10f)
        );

        // Act
        var window = series.Window(0, 2);

        // Assert
        window.HighPrice.ShouldBe(117.5, 1e-9);
        window.LowPrice.ShouldBe(97.5, 1e-9);
        window.HighVolume.ShouldBe(480, 1e-9);
        window.LowVolume.ShouldBe(480, 1e-9);
        window.PairedBars.ShouldBe(2 * FlipMarketData.BarsPerHour);
    }

    [Fact]
    public void Window_ShouldOnlyCountBucketsWhereBothSidesTraded()
    {
        // Arrange
        var series = FlipTestData.Series(
            1,
            i => i % 2 == 0 ? new FlipBar(110f, 10f, 100f, 10f) : new FlipBar(110f, 10f, 0f, 0f)
        );

        // Act
        var window = series.Window(0, 1);

        // Assert
        window.PairedBars.ShouldBe(FlipMarketData.BarsPerHour / 2);
    }

    [Fact]
    public void Window_WhenRangeIsOutsideTheSeries_ShouldClampAndReturnEmpty()
    {
        // Arrange
        var series = FlipTestData.Bouncing(2, 100f, 110f);

        // Act
        var window = series.Window(-5, 0);

        // Assert
        window.HighVolume.ShouldBe(0);
        window.HighPrice.ShouldBe(0);
    }
}
