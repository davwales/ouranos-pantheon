using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Signals;

public sealed class SignalTests
{
    private readonly IFixture _fixture = new Fixture();

    public SignalTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var type = _fixture.Create<SignalType>();
        var value = _fixture.Create<decimal>();

        // Act
        var signal = Signal.Create(marketId, symbolId, type, value);

        // Assert
        signal.MarketId.ShouldBe(marketId);
        signal.SymbolId.ShouldBe(symbolId);
        signal.Type.ShouldBe(type);
        signal.Value.ShouldBe(value);
        signal.ComputedAt.ShouldBe(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldAutoGenerateId()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();

        // Act
        var signal1 = Signal.Create(marketId, symbolId, SignalType.Rsi, 0.5m);
        var signal2 = Signal.Create(marketId, symbolId, SignalType.Rsi, 0.5m);

        // Assert
        signal1.Id.ShouldNotBe(signal2.Id);
    }

    [Fact]
    public void Create_WhenNullNavigations_ShouldSucceed()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();

        // Act
        var signal = Signal.Create(marketId, symbolId, SignalType.BollingerBands, 0m, null, null);

        // Assert
        signal.ShouldNotBeNull();
    }

    [Fact]
    public void Create_WhenMarketNavigationMismatch_ShouldThrow()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var wrongMarket = Market.Create(
            new Id<Market>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        // Act
        var create = () => Signal.Create(marketId, symbolId, SignalType.Rsi, 0m, wrongMarket);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenSymbolNavigationMismatch_ShouldThrow()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var market = Market.Create(
            marketId,
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );
        var wrongSymbol = Symbol.Create(
            new Id<Symbol>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        // Act
        var create = () => Signal.Create(marketId, symbolId, SignalType.Rsi, 0m, market, wrongSymbol);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}
