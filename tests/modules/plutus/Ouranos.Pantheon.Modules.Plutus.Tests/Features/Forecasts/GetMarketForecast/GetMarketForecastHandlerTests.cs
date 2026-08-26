using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.GetMarketForecast;

public sealed class GetMarketForecastHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetMarketForecastHandler _handler;
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetMarketForecastHandler> _logger = Substitute.For<
        ILogger<GetMarketForecastHandler>
    >();

    public GetMarketForecastHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetMarketForecastHandler(
            _logger,
            _dbContext,
            Options.Create(new QueryOptions())
        );
    }

    [Fact]
    public async Task Handle_WhenSkipExceedsMax_ShouldThrow()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            new Taxes(null)
        );
        await _dbContext.SeedData(market);

        var query = new GetMarketForecastInput(market.Id, Skip: 99999, Take: 10);

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenTakeExceedsMax_ShouldThrow()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            new Taxes(null)
        );
        await _dbContext.SeedData(market);

        var query = new GetMarketForecastInput(market.Id, Take: 9999);

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenMarketNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetMarketForecastInput(new Id<Market>(_fixture.Create<string>()), Take: 10);

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetMarketForecastInput(new Id<Market>(_fixture.Create<string>()), Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void GetMarketForecastInput_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var input = new GetMarketForecastInput(marketId, "price", "asc", 0, 5, ["Name:eq:Gold"]);

        // Assert
        input.MarketId.ShouldBe(marketId);
        input.SortField.ShouldBe("price");
        input.SortDirection.ShouldBe("asc");
        input.Skip.ShouldBe(0);
        input.Take.ShouldBe(5);
        input.Filter.ShouldNotBeNull();
    }

    [Fact]
    public void GetMarketForecastPredictionResponse_AllProperties_ShouldBeAccessible()
    {
        // Act
        var prediction = new GetMarketForecastPredictionResponse(
            AveragePrice: 100m,
            MinPrice: 90m,
            MaxPrice: 110m,
            Volume: 500m,
            Margin: 20m,
            Gain: 10000m,
            AveragePriceDelta: 5m,
            MinPriceDelta: 2m,
            MaxPriceDelta: 8m,
            VolumeDelta: 50m,
            GainDelta: 1000m
        );

        // Assert
        prediction.AveragePrice.ShouldBe(100m);
        prediction.MinPrice.ShouldBe(90m);
        prediction.MaxPrice.ShouldBe(110m);
        prediction.Volume.ShouldBe(500m);
        prediction.Margin.ShouldBe(20m);
        prediction.Gain.ShouldBe(10000m);
        prediction.AveragePriceDelta.ShouldBe(5m);
        prediction.MinPriceDelta.ShouldBe(2m);
        prediction.MaxPriceDelta.ShouldBe(8m);
        prediction.VolumeDelta.ShouldBe(50m);
        prediction.GainDelta.ShouldBe(1000m);
    }

    [Fact]
    public void GetMarketForecastResponse_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var prediction = new GetMarketForecastPredictionResponse(
            100m,
            90m,
            110m,
            500m,
            20m,
            10000m,
            5m,
            2m,
            8m,
            50m,
            1000m
        );
        var forecastId = new Id<Forecast>(Guid.NewGuid().ToString());
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var latest = new ForecastPoint(100m, 90m, 110m, 500m);

        // Act
        var response = new GetMarketForecastResponse(
            forecastId,
            marketId,
            symbolId,
            "Gold",
            null,
            latest,
            prediction,
            prediction,
            prediction,
            prediction,
            prediction,
            prediction,
            prediction
        );

        // Assert
        response.Id.ShouldBe(forecastId);
        response.MarketId.ShouldBe(marketId);
        response.SymbolId.ShouldBe(symbolId);
        response.SymbolName.ShouldBe("Gold");
        response.Latest.ShouldNotBeNull();
        response.DayOne.ShouldNotBeNull();
        response.DayTwo.ShouldNotBeNull();
        response.DayThree.ShouldNotBeNull();
        response.DayFour.ShouldNotBeNull();
        response.DayFive.ShouldNotBeNull();
        response.DaySix.ShouldNotBeNull();
        response.DaySeven.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenMultipleForecastsForSameSymbol_ShouldReturnOnlyLatest()
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

        var baseTime = DateTimeOffset.UtcNow;
        static List<ForecastPoint> MakePredictions() =>
            [.. Enumerable.Range(0, 7).Select(_ => new ForecastPoint(105m, 95m, 115m, 520m))];

        var olderForecast = Forecast
            .Create(
                new Id<Forecast>(Guid.NewGuid().ToString()),
                market.Id,
                symbol.Id,
                new ForecastPoint(100m, 90m, 110m, 500m),
                MakePredictions()
            )
            .WithCreatedAt(baseTime.AddDays(-5));

        var latestForecast = Forecast
            .Create(
                new Id<Forecast>(Guid.NewGuid().ToString()),
                market.Id,
                symbol.Id,
                new ForecastPoint(102m, 92m, 112m, 510m),
                MakePredictions()
            )
            .WithCreatedAt(baseTime);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(olderForecast);
        await _dbContext.SeedData(latestForecast);

        var query = new GetMarketForecastInput(market.Id, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Single().Id.ShouldBe(latestForecast.Id);
    }
}
