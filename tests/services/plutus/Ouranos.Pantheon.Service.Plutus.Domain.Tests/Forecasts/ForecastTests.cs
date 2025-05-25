using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Tests.Forecasts;

public sealed class ForecastTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Forecast>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var symbolName = _fixture.Create<string>();
        var latest = _fixture.Create<ForecastPoint>();
        var predictions = _fixture.CreateMany<ForecastPoint>().ToList();

        // Act
        var forecast = new Forecast(
            id,
            marketId,
            symbolId,
            symbolName,
            null,
            latest,
            predictions
        );

        // Assert
        forecast.Id.ShouldBe(id);
        forecast.MarketId.ShouldBe(marketId);
        forecast.SymbolId.ShouldBe(symbolId);
        forecast.SymbolName.ShouldBe(symbolName);
        forecast.SymbolSubcode.ShouldBeNull();
        forecast.Latest.ShouldBe(latest);
        forecast.Predictions.ShouldBe(predictions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidSymbolName_ShouldThrowArgumentException(string? symbolName)
    {
        // Arrange
        var id = new Id<Forecast>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var latest = _fixture.Create<ForecastPoint>();
        var predictions = _fixture.CreateMany<ForecastPoint>().ToList();

        // Act
        var action = () => new Forecast(
            id,
            marketId,
            symbolId,
            symbolName!,
            null,
            latest,
            predictions
        );

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullLatest_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Forecast>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var predictions = _fixture.CreateMany<ForecastPoint>().ToList();

        // Act
        var action = () => new Forecast(
            id,
            marketId,
            symbolId,
            "Test",
            null,
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
        var id = new Id<Forecast>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var latest = _fixture.Create<ForecastPoint>();

        // Act
        var action = () => new Forecast(
            id,
            marketId,
            symbolId,
            "Test",
            null,
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
        var id = new Id<Forecast>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var symbolId = new Id<Symbol>(_fixture.Create<string>());
        var latest = _fixture.Create<ForecastPoint>();

        // Act
        var action = () => new Forecast(
            id,
            marketId,
            symbolId,
            "Test",
            null,
            latest,
            []
        );

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}