using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks;

public sealed class StocksDataLoaderOptionsTests
{
    [Fact]
    public void StocksDataLoaderOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new StocksDataLoaderOptions();

        // Assert
        options.ApiKey.ShouldNotBeNull();
        options.ApiSecret.ShouldNotBeNull();
        options.Symbols.ShouldNotBeNull();
        StocksDataLoaderOptions.SectionName.ShouldBe("Stocks");
    }
}
