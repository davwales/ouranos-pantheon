using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.MarketTradeSnapshot;

public sealed class MarketTradeSnapshotOptionsTests
{
    [Fact]
    public void MarketTradeSnapshotOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new MarketTradeSnapshotOptions();

        // Assert
        options.BatchSize.ShouldBe(500);
        MarketTradeSnapshotOptions.SectionName.ShouldBe("MarketTradeSnapshot");
    }
}
