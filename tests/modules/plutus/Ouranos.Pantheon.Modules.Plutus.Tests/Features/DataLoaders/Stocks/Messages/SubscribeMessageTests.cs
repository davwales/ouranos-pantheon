using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks.Messages;

public sealed class SubscribeMessageTests
{
    [Fact]
    public void SubscribeMessage_AllProperties_ShouldBeAccessible()
    {
        // Arrange & Act
        var message = new SubscribeMessage(
            Trades: ["AAPL"],
            Quotes: ["MSFT"],
            Bars: ["GOOG"]
        );

        // Assert
        message.Trades.ShouldContain("AAPL");
        message.Quotes.ShouldContain("MSFT");
        message.Bars.ShouldContain("GOOG");
        message.Action.ShouldBe("subscribe");
    }
}
