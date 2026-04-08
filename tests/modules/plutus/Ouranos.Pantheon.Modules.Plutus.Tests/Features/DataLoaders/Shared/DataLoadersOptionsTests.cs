using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Shared;

public sealed class DataLoadersOptionsTests
{
    [Fact]
    public void DataLoadersOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new DataLoadersOptions();

        // Assert
        options.Ffxiv.ShouldNotBeNull();
        options.Osrs.ShouldNotBeNull();
        options.Stocks.ShouldNotBeNull();
        options.Consumer.ShouldNotBeNull();
        DataLoadersOptions.SectionName.ShouldBe("DataLoaders");
    }
}
