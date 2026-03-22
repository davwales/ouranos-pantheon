using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Trades;

public sealed class TradeTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Trade>(_fixture.Create<string>());
        var market = _fixture.Create<Market>();
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );
        var price = _fixture.Create<decimal>();
        var volume = _fixture.Create<decimal>();
        var timestamp = _fixture.Create<DateTimeOffset>();

        // Act
        var trade = Trade.Create(id, symbol.Id, price, volume, timestamp);

        // Assert
        trade.Id.ShouldBe(id);
        trade.SymbolId.ShouldBe(symbol.Id);
        trade.Price.ShouldBe(price);
        trade.Volume.ShouldBe(volume);
        trade.Timestamp.ShouldBe(timestamp);
    }
}
