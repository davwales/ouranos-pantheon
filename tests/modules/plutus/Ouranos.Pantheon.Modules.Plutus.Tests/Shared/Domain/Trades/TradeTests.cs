using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

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

    [Fact]
    public void Create_WhenSymbolNavigationMismatch_ShouldThrow()
    {
        // Arrange
        var tradeId = new Id<Trade>(_fixture.Create<string>());
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var market = _fixture.Create<Market>();
        var wrongSymbol = Symbol.Create(
            new Id<Symbol>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        // Act
        var create = () =>
            Trade.Create(tradeId, symbolId, 100m, 10m, DateTimeOffset.UtcNow, wrongSymbol);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void TradeMetadata_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var marketId = new Id<Market>(_fixture.Create<string>());
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var additionalFields = new AdditionalFields(
            Limit: 500m,
            HighAlch: 100,
            LowAlch: 80,
            Exchange: "NYSE",
            Tape: "A",
            ExternalTradeId: "ext-1"
        );

        // Act
        var metadata = new TradeMetadata(marketId, symbolId, "Gold", "AU", "G", additionalFields);

        // Assert
        metadata.MarketId.ShouldBe(marketId);
        metadata.SymbolId.ShouldBe(symbolId);
        metadata.SymbolName.ShouldBe("Gold");
        metadata.SymbolCode.ShouldBe("AU");
        metadata.SymbolSubcode.ShouldBe("G");
        metadata.AdditionalFields.ShouldNotBeNull();
        metadata.AdditionalFields.Limit.ShouldBe(500m);
        metadata.AdditionalFields.HighAlch.ShouldBe(100);
        metadata.AdditionalFields.LowAlch.ShouldBe(80);
        metadata.AdditionalFields.Exchange.ShouldBe("NYSE");
        metadata.AdditionalFields.Tape.ShouldBe("A");
        metadata.AdditionalFields.ExternalTradeId.ShouldBe("ext-1");
    }

    [Fact]
    public void Symbol_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var trade = Trade.Create(
            new Id<Trade>(_fixture.Create<string>()),
            new Id<Symbol>(_fixture.Create<string>()),
            100m,
            10m,
            DateTimeOffset.UtcNow
        );

        // Act
        var access = () => _ = trade.Symbol;

        // Assert
        access.ShouldThrow<Exception>();
    }

    [Fact]
    public void Create_WhenValidSymbolProvided_ShouldExposeSymbolNavigation()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );
        var tradeId = new Id<Trade>(Guid.NewGuid().ToString());

        // Act
        var trade = Trade.Create(tradeId, symbol.Id, 100m, 10m, DateTimeOffset.UtcNow, symbol);

        // Assert
        trade.Symbol.ShouldBe(symbol);
    }

    [Fact]
    public void PriceBucket_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var bucket = new PriceBucket(now, 100m, 90m, 110m, 500m);

        // Assert
        bucket.BucketStart.ShouldBe(now);
        bucket.AveragePrice.ShouldBe(100m);
        bucket.MinPrice.ShouldBe(90m);
        bucket.MaxPrice.ShouldBe(110m);
        bucket.Volume.ShouldBe(500m);
    }

    [Fact]
    public void SignalComputeContext_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var snapshot = new MarketTradeSnapshot(
            marketId,
            symbolId,
            TimeFrame.OneDay,
            1000m,
            90m,
            110m,
            10m,
            5,
            100m,
            1m
        );
        var bucket = new PriceBucket(DateTimeOffset.UtcNow, 100m, 90m, 110m, 500m);

        // Act
        var context = new SignalComputeContext(
            symbolId,
            marketId,
            0.05m,
            500m,
            snapshot,
            snapshot,
            snapshot,
            [bucket]
        );

        // Assert
        context.SymbolId.ShouldBe(symbolId);
        context.MarketId.ShouldBe(marketId);
        context.TaxRate.ShouldBe(0.05m);
        context.Limit.ShouldBe(500m);
        context.ShortSnapshot.ShouldNotBeNull();
        context.MediumSnapshot.ShouldNotBeNull();
        context.LongSnapshot.ShouldNotBeNull();
        context.PriceBuckets.Count.ShouldBe(1);
    }
}
