using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv.XivApi;

public sealed class XivApiOptionsTests
{
    [Fact]
    public void XivApiOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new XivApiOptions();

        // Assert
        options.BaseAddress.ShouldBe(string.Empty);
    }

    [Fact]
    public void XivApiOptions_WhenConstructedWithValues_ShouldSetProperties()
    {
        // Act
        var options = new XivApiOptions("https://xivapi.com", 60);

        // Assert
        options.BaseAddress.ShouldBe("https://xivapi.com");
        options.ItemCacheMinutes.ShouldBe(60);
    }
}
