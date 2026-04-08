using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv;

public sealed class FfxivDataLoaderOptionsTests
{
    [Fact]
    public void FfxivDataLoaderOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new FfxivDataLoaderOptions();

        // Assert
        options.WebSocket.ShouldNotBeNull();
        options.XivApi.ShouldNotBeNull();
        options.Worlds.ShouldNotBeNull();
        FfxivDataLoaderOptions.SectionName.ShouldBe("Ffxiv");
    }
}
