using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;
using Ouranos.Pantheon.Plutus.Service.Application.Options;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Infra.OuranosMl.Forecasts;
using Shouldly;
using Xunit;

namespace Ouranos.Pantheon.Plutus.Service.Infra.OuranosMl.Tests.Forecasts;

public sealed class GetForecastPredictionsTests
{
    private readonly IOuranosMachineLearningClient _client;
    private readonly IFixture _fixture;
    private readonly IOptions<ForecastingOptions> _forecastingOptions;
    private readonly GetForecastPredictions _getForecastPredictions;

    public GetForecastPredictionsTests()
    {
        _fixture = new Fixture();
        _client = Substitute.For<IOuranosMachineLearningClient>();
        _forecastingOptions = Substitute.For<IOptions<ForecastingOptions>>();

        _getForecastPredictions = new GetForecastPredictions(
            Substitute.For<ILogger<GetForecastPredictions>>(),
            _client,
            _forecastingOptions
        );
    }

    [Fact]
    public async Task GetPredictionsAsync_WhenHappyPath_ShouldSendExpectedRequest()
    {
        // Arrange
        var historicalData = _fixture.CreateMany<List<ForecastPoint>>().ToList();
        var forecasts = _fixture.CreateMany<List<Core.Infra.OuranosMl.Dtos.ForecastPoint>>().ToList();

        var options = new ForecastingOptions();
        _forecastingOptions.Value.Returns(options);

        _client
            .GetPlutusForecasts(
                Arg.Is<GetPlutusForecastsRequest>(x =>
                    x.NumPredictions == options.NumPredictions &&
                    x.Points.Count == historicalData.Count &&
                    x.Points.Select((p, i) => p.Count == historicalData[i].Count).All(e => e)
                ),
                Arg.Any<CancellationToken>()
            )
            .Returns(forecasts);

        // Act
        var actualResponse = await _getForecastPredictions.GetPredictionsAsync(historicalData);

        // Assert
        actualResponse.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetPredictionsAsync_WhenHappyPath_ShouldReturnExpectedForecasts()
    {
        // Arrange
        var historicalData = _fixture.CreateMany<List<ForecastPoint>>().ToList();
        var forecasts = _fixture.CreateMany<List<Core.Infra.OuranosMl.Dtos.ForecastPoint>>().ToList();

        var options = new ForecastingOptions();
        _forecastingOptions.Value.Returns(options);

        _client
            .GetPlutusForecasts(Arg.Any<GetPlutusForecastsRequest>(), Arg.Any<CancellationToken>())
            .Returns(forecasts);

        // Act
        var actualResponse = await _getForecastPredictions.GetPredictionsAsync(historicalData);

        // Assert
        actualResponse.ShouldNotBeNull();
        actualResponse.Count.ShouldBe(forecasts.Count);
        actualResponse.ShouldBe(
            forecasts.Select(f =>
                new List<ForecastPoint>(
                    f.Select(x =>
                        new ForecastPoint(
                            x.AveragePrice,
                            x.MinPrice,
                            x.MaxPrice,
                            x.Volume
                        )
                    )
                )
            )
        );
    }

    [Fact]
    public async Task GetPredictionsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var historicalData = _fixture.CreateMany<List<ForecastPoint>>().ToList();
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _getForecastPredictions.GetPredictionsAsync(historicalData, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}