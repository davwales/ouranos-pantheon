using Ouranos.Pantheon.Service.Plutus.Application.Options;

namespace Ouranos.Pantheon.Service.Plutus.Application.Tests.Options;

public sealed class ForecastingOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetExpectedDefaults()
    {
        // Act
        var options = new ForecastingOptions();

        // Assert
        options.IsEnabled.ShouldBeTrue();
        options.BatchSize.ShouldBe(100);
        options.NumPredictions.ShouldBe(7);
        options.SequenceLength.ShouldBe(30);
    }
}