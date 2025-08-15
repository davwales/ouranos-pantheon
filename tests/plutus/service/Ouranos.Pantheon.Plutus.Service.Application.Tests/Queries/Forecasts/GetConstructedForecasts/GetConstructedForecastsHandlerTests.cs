using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetConstructedForecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketForecast;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Queries.Forecasts.GetConstructedForecasts;

public sealed class GetConstructedForecastsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IGetForecastPredictions _getForecastPredictions = Substitute.For<IGetForecastPredictions>();
    private readonly GetConstructedForecastsHandler _handler;
    private readonly ILogger<GetMarketForecastHandler> _logger = Substitute.For<ILogger<GetMarketForecastHandler>>();
    private readonly IPlutusUnitOfWork _unitOfWork = Substitute.For<IPlutusUnitOfWork>();

    public GetConstructedForecastsHandlerTests()
    {
        _handler = new GetConstructedForecastsHandler(_logger, _getForecastPredictions, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnForecasts()
    {
        // Arrange
        var symbols = _fixture.CreateMany<Symbol>().ToList();
        var historicalData = symbols.ToDictionary(x => x.Id, _ => _fixture.CreateMany<ForecastPoint>().ToList());
        var query = new GetConstructedForecastsInput(symbols, historicalData);
        var expectedPredictions = _fixture.Create<List<List<ForecastPoint>>>();
        var expectedId = new Id<Forecast>(_fixture.Create<string>());

        _getForecastPredictions
            .GetPredictionsAsync(
                Arg.Any<List<List<ForecastPoint>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(expectedPredictions);

        _unitOfWork.Forecasts.CreateId().Returns(expectedId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<WrapperResponse<List<Forecast>>>();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(query.HistoricalData.Count);

        await _getForecastPredictions.Received(1).GetPredictionsAsync(
            Arg.Is<List<List<ForecastPoint>>>(data =>
                data.Count == query.HistoricalData.Count
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = _fixture.Create<GetConstructedForecastsInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}