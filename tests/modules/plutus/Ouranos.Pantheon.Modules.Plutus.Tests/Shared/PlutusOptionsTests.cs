using Ouranos.Pantheon.Modules.Plutus.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared;

public sealed class PlutusOptionsTests
{
    [Fact]
    public void PlutusOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new PlutusOptions();

        // Assert
        options.DataLoaders.ShouldNotBeNull();
        options.MarketTradeSnapshot.ShouldNotBeNull();
        options.Forecasting.ShouldNotBeNull();
        PlutusOptions.SectionName.ShouldBe("Ouranos:Plutus");
    }
}
