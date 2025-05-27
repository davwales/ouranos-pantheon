using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetConstructedForecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Markets.GetMarketForecast;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Tests.Queries.Forecasts.GetConstructedForecasts;

public sealed class GetConstructedForecastsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IRepository<Forecast> _forecastRepository = Substitute.For<IRepository<Forecast>>();
    private readonly IGetForecastPredictions _getForecastPredictions = Substitute.For<IGetForecastPredictions>();
    private readonly GetConstructedForecastsHandler _handler;
    private readonly ILogger<GetMarketForecastHandler> _logger = Substitute.For<ILogger<GetMarketForecastHandler>>();

    public GetConstructedForecastsHandlerTests()
    {
        _handler = new GetConstructedForecastsHandler(_logger, _getForecastPredictions, _forecastRepository);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnForecasts()
    {
        // Arrange
        var symbols = _fixture.CreateMany<Symbol>().ToList();
        var historicalData = symbols.ToDictionary(x => x.Id, _ => _fixture.CreateMany<ForecastPoint>().ToList());
        var command = new GetConstructedForecastsInput(symbols, historicalData);
        var expectedPredictions = _fixture.Create<List<List<ForecastPoint>>>();
        var expectedId = new Id<Forecast>(_fixture.Create<string>());

        _getForecastPredictions
            .GetPredictionsAsync(
                Arg.Any<List<List<ForecastPoint>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(expectedPredictions);

        _forecastRepository.CreateId().Returns(expectedId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<WrapperResponse<List<Forecast>>>();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(command.HistoricalData.Count);

        await _getForecastPredictions.Received(1).GetPredictionsAsync(
            Arg.Is<List<List<ForecastPoint>>>(data =>
                data.Count == command.HistoricalData.Count
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = _fixture.Create<GetConstructedForecastsInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}