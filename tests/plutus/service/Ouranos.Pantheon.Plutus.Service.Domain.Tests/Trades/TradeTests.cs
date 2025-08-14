using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Tests.Trades;

public sealed class TradeTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Trade>(_fixture.Create<string>());
        var symbol = _fixture.Create<Symbol>();
        var price = _fixture.Create<decimal>();
        var volume = _fixture.Create<decimal>();
        var timestamp = _fixture.Create<DateTimeOffset>();

        // Act
        var trade = new Trade(id, symbol.Id, price, volume, timestamp)
        {
            Symbol = symbol
        };

        // Assert
        trade.Id.ShouldBe(id);
        trade.SymbolId.ShouldBe(symbol.Id);
        trade.Price.ShouldBe(price);
        trade.Volume.ShouldBe(volume);
        trade.CreatedAt.ShouldBe(timestamp);
    }
}