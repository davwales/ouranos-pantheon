using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Osrs;

public sealed class OsrsWikiOptionsTests
{
    [Fact]
    public void OsrsWikiOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new OsrsWikiOptions();

        // Assert
        options.BaseAddress.ShouldBe(string.Empty);
        options.UserAgent.ShouldBe(string.Empty);
    }
}
