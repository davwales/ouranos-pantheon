using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Models.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Options;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetHistoricalData;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Queries.Forecasts.GetHistoricalData;

public sealed class GetHistoricalDataHandlerTests
{
    private readonly IBucketHistoricalData _bucketHistoricalData = Substitute.For<IBucketHistoricalData>();
    private readonly IOptions<ForecastingOptions> _forecastingOptions = Substitute.For<IOptions<ForecastingOptions>>();
    private readonly GetHistoricalDataHandler _handler;
    private readonly ILogger<GetHistoricalDataHandler> _logger = Substitute.For<ILogger<GetHistoricalDataHandler>>();
    private readonly IQueryExecutor _queryExecutor = Substitute.For<IQueryExecutor>();
    private readonly IPlutusUnitOfWork _unitOfWork = Substitute.For<IPlutusUnitOfWork>();

    public GetHistoricalDataHandlerTests()
    {
        _handler = new GetHistoricalDataHandler(
            _logger,
            _bucketHistoricalData,
            _unitOfWork,
            _queryExecutor,
            _forecastingOptions
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnHistoricalData()
    {
        // Arrange
        var symbolIds = new Fixture().CreateMany<Id<Symbol>>().ToList();
        var query = new GetHistoricalDataInput(symbolIds);
        var expectedData = symbolIds.ToDictionary(id => id, _ => new Fixture().CreateMany<ForecastPoint>().ToList());
        var historicalDtos = expectedData.Select(x => new HistoricalDataDto(x.Key, x.Value));

        _forecastingOptions.Value.Returns(
            new ForecastingOptions
            {
                SequenceLength = 30
            }
        );

        _queryExecutor
            .ToList(Arg.Any<IQueryable<HistoricalDataDto>>(), Arg.Any<CancellationToken>())
            .Returns(historicalDtos.ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>>();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(expectedData);

        _unitOfWork.Trades.Received(1).AsQueryable(Arg.Any<CancellationToken>());

        await _queryExecutor.Received(1).ToList(
            Arg.Any<IQueryable<HistoricalDataDto>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new Fixture().Create<GetHistoricalDataInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}