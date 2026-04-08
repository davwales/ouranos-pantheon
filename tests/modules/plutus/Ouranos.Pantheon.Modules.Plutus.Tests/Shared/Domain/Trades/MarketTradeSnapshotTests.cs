using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Trades;

public sealed class MarketTradeSnapshotTests
{
    private readonly IFixture _fixture = new Fixture();

    public MarketTradeSnapshotTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var timeFrame = _fixture.Create<TimeFrame>();
        var totalSpent = _fixture.Create<decimal>();
        var minPrice = _fixture.Create<decimal>();
        var maxPrice = _fixture.Create<decimal>();
        var totalVolume = _fixture.Create<decimal>();
        var numTransactions = _fixture.Create<int>();
        var limit = _fixture.Create<decimal>();
        var tax = _fixture.Create<decimal>();

        // Act
        var snapshot = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            timeFrame,
            totalSpent,
            minPrice,
            maxPrice,
            totalVolume,
            numTransactions,
            limit,
            tax
        );

        // Assert
        snapshot.MarketId.ShouldBe(marketId);
        snapshot.SymbolId.ShouldBe(symbolId);
        snapshot.TimeFrame.ShouldBe(timeFrame);
        snapshot.TotalSpent.ShouldBe(totalSpent);
        snapshot.MinPrice.ShouldBe(minPrice);
        snapshot.MaxPrice.ShouldBe(maxPrice);
        snapshot.TotalVolume.ShouldBe(totalVolume);
        snapshot.NumTransactions.ShouldBe(numTransactions);
        snapshot.Limit.ShouldBe(limit);
        snapshot.Tax.ShouldBe(tax);
    }

    [Fact]
    public void Create_ShouldAutoGenerateId()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();

        // Act
        var snapshot1 = MarketTradeSnapshot.Create(marketId, symbolId, TimeFrame.OneDay, 0, 0, 0, 0, 0, 0, 0);
        var snapshot2 = MarketTradeSnapshot.Create(marketId, symbolId, TimeFrame.OneDay, 0, 0, 0, 0, 0, 0, 0);

        // Assert
        snapshot1.Id.ShouldNotBe(snapshot2.Id);
    }

    [Fact]
    public void Create_WhenMarketNavigationMismatch_ShouldThrow()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var wrongMarket = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        // Act
        var act = () => MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneDay,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            market: wrongMarket
        );

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenSymbolNavigationMismatch_ShouldThrow()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var wrongSymbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            marketId,
            new AdditionalFields()
        );

        // Act
        var act = () => MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneDay,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            symbol: wrongSymbol
        );

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Market_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var snapshot = MarketTradeSnapshot.Create(
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            TimeFrame.OneDay,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        );

        // Act
        var access = () => _ = snapshot.Market;

        // Assert
        access.ShouldThrow<Exception>();
    }

    [Fact]
    public void Symbol_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var snapshot = MarketTradeSnapshot.Create(
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            TimeFrame.OneDay,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        );

        // Act
        var access = () => _ = snapshot.Symbol;

        // Assert
        access.ShouldThrow<Exception>();
    }

    [Fact]
    public void Create_WhenValidMarketAndSymbolProvided_ShouldExposeNavigationProperties()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            new Taxes(null)
        );
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        // Act
        var snapshot = MarketTradeSnapshot.Create(
            market.Id,
            symbol.Id,
            TimeFrame.OneDay,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            market: market,
            symbol: symbol
        );

        // Assert
        snapshot.Market.ShouldBe(market);
        snapshot.Symbol.ShouldBe(symbol);
    }
}
