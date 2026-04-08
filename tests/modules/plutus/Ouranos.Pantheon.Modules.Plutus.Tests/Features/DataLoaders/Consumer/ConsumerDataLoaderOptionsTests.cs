using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Consumer;

public sealed class ConsumerDataLoaderOptionsTests
{
    [Fact]
    public void ConsumerDataLoaderOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new ConsumerDataLoaderOptions();

        // Assert
        options.MarketMap.ShouldNotBeNull();
        ConsumerDataLoaderOptions.SectionName.ShouldBe("Consumer");
    }
}
