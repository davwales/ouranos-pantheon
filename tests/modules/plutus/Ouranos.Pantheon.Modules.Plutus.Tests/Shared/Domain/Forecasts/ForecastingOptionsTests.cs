using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Forecasts;

public sealed class ForecastingOptionsTests
{
    [Fact]
    public void ForecastingOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new ForecastingOptions();

        // Assert
        options.IsEnabled.ShouldBeTrue();
        options.NumPredictions.ShouldBe(7);
        options.HistoryDays.ShouldBe(30);
        options.BatchSize.ShouldBe(500);
        options.ModelName.ShouldBe("plutus-forecasting-v1");
    }
}
