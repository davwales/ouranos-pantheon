using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Forecasts;

public sealed class ForecastTests
{
    private readonly IFixture _fixture = new Fixture();

    private Symbol CreateSymbol()
    {
        var market = _fixture.Create<Market>();
        return Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );
    }

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = _fixture.Create<Id<Forecast>>();
        var market = _fixture.Create<Market>();
        var symbol = CreateSymbol();
        var latest = _fixture.Create<ForecastPoint>();
        var predictions = _fixture.CreateMany<ForecastPoint>().ToList();

        // Act
        var forecast = Forecast.Create(id, market.Id, symbol.Id, latest, predictions);

        // Assert
        forecast.Id.ShouldBe(id);
        forecast.MarketId.ShouldBe(market.Id);
        forecast.SymbolId.ShouldBe(symbol.Id);
        forecast.Latest.ShouldBe(latest);
        forecast.Predictions.ShouldBe(predictions);
    }

    [Fact]
    public void Constructor_WhenNullLatest_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Forecast>>();
        var market = _fixture.Create<Market>();
        var symbol = CreateSymbol();
        var predictions = _fixture.CreateMany<ForecastPoint>().ToList();

        // Act
        var action = () => Forecast.Create(id, market.Id, symbol.Id, null!, predictions);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullPredictions_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Forecast>>();
        var market = _fixture.Create<Market>();
        var symbol = CreateSymbol();
        var latest = _fixture.Create<ForecastPoint>();

        // Act
        var action = () => Forecast.Create(id, market.Id, symbol.Id, latest, null!);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenEmptyPredictions_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Forecast>>();
        var market = _fixture.Create<Market>();
        var symbol = CreateSymbol();
        var latest = _fixture.Create<ForecastPoint>();

        // Act
        var action = () => Forecast.Create(id, market.Id, symbol.Id, latest, []);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}
