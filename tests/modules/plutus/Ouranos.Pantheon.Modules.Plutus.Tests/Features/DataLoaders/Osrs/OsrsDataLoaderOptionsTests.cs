using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Osrs;

public sealed class OsrsDataLoaderOptionsTests
{
    [Fact]
    public void OsrsDataLoaderOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new OsrsDataLoaderOptions();

        // Assert
        options.Wiki.ShouldNotBeNull();
        OsrsDataLoaderOptions.SectionName.ShouldBe("Osrs");
    }
}
