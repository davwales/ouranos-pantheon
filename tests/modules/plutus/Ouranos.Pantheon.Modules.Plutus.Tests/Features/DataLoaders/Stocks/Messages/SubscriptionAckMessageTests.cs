using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks.Messages;

public sealed class SubscriptionAckMessageTests
{
    [Fact]
    public void SubscriptionAckMessage_AllProperties_ShouldBeAccessible()
    {
        // Arrange & Act
        var message = new SubscriptionAckMessage(
            Trades: ["AAPL"],
            Quotes: ["MSFT"],
            Bars: ["GOOG"],
            UpdatedBars: ["TSLA"],
            DailyBars: ["AMZN"],
            Statuses: ["META"],
            Lulds: ["NVDA"],
            Corrections: ["NFLX"],
            CancelErrors: ["AMD"]
        );

        // Assert
        message.Trades.ShouldContain("AAPL");
        message.Quotes.ShouldContain("MSFT");
        message.Bars.ShouldContain("GOOG");
        message.UpdatedBars.ShouldContain("TSLA");
        message.DailyBars.ShouldContain("AMZN");
        message.Statuses.ShouldContain("META");
        message.Lulds.ShouldContain("NVDA");
        message.Corrections.ShouldContain("NFLX");
        message.CancelErrors.ShouldContain("AMD");
        SubscriptionAckMessage.TypeIndicator.ShouldBe("subscription");
    }
}
