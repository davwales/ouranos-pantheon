using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks.Messages;

public sealed class AlpacaTradeMessageTests
{
    [Fact]
    public void AlpacaTradeMessage_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var ts = DateTimeOffset.UtcNow;

        // Act
        var message = new AlpacaTradeMessage(
            SymbolCode: "AAPL",
            TradeId: 42,
            ExchangeCode: "NYSE",
            Price: 150.25m,
            Size: 100,
            Conditions: ["@"],
            Timestamp: ts,
            Tape: "C",
            Vwap: 150.10m
        );

        // Assert
        message.SymbolCode.ShouldBe("AAPL");
        message.TradeId.ShouldBe(42);
        message.ExchangeCode.ShouldBe("NYSE");
        message.Price.ShouldBe(150.25m);
        message.Size.ShouldBe(100);
        message.Conditions.ShouldNotBeNull();
        message.Timestamp.ShouldBe(ts);
        message.Tape.ShouldBe("C");
        message.Vwap.ShouldBe(150.10m);
        AlpacaTradeMessage.TypeIndicator.ShouldBe("t");
    }
}
