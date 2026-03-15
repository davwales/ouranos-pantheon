using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Forecasts;

public sealed class ForecastTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = _fixture.Create<Id<Forecast>>();
        var market = _fixture.Create<Market>();
        var symbol = _fixture.Create<Symbol>();
        var latest = _fixture.Create<ForecastPoint>();
        var predictions = _fixture.CreateMany<ForecastPoint>().ToList();

        // Act
        var forecast = Forecast.Create(
            id,
            market,
            symbol,
            latest,
            predictions
        );

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
        var symbol = _fixture.Create<Symbol>();
        var predictions = _fixture.CreateMany<ForecastPoint>().ToList();

        // Act
        var action = () => Forecast.Create(
            id,
            market,
            symbol,
            null!,
            predictions
        );

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullPredictions_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Forecast>>();
        var market = _fixture.Create<Market>();
        var symbol = _fixture.Create<Symbol>();
        var latest = _fixture.Create<ForecastPoint>();

        // Act
        var action = () => Forecast.Create(
            id,
            market,
            symbol,
            latest,
            null!
        );

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenEmptyPredictions_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Forecast>>();
        var market = _fixture.Create<Market>();
        var symbol = _fixture.Create<Symbol>();
        var latest = _fixture.Create<ForecastPoint>();

        // Act
        var action = () => Forecast.Create(
            id,
            market,
            symbol,
            latest,
            []
        );

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}
