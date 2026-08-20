using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.GetAllForecasts;

public sealed class GetAllForecastsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllForecastsHandler _handler;
    private readonly ILogger<GetAllForecastsHandler> _logger = Substitute.For<
        ILogger<GetAllForecastsHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetAllForecastsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllForecastsHandler(
            _logger,
            _dbContext,
            Options.Create(new QueryOptions())
        );
    }

    [Fact]
    public async Task Handle_WhenNoForecasts_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetAllForecastsInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenForecastsExist_ShouldReturnPagedForecasts()
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

        var forecast = Forecast.Create(
            new Id<Forecast>(Guid.NewGuid().ToString()),
            market.Id,
            symbol.Id,
            new ForecastPoint(100m, 90m, 110m, 500m),
            [new ForecastPoint(105m, 95m, 115m, 520m)],
            symbol
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(forecast);

        var query = new GetAllForecastsInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        result.TotalCount.ShouldBe(1);
        var item = result.Items.First();
        item.Id.ShouldBe(forecast.Id);
        item.MarketId.ShouldBe(market.Id);
        item.SymbolId.ShouldBe(symbol.Id);
        item.SymbolName.ShouldBe(symbol.Name);
        item.Latest.ShouldNotBeNull();
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
        var olderForecast = Forecast
            .Create(
                new Id<Forecast>(Guid.NewGuid().ToString()),
                market.Id,
                symbol.Id,
                new ForecastPoint(100m, 90m, 110m, 500m),
                [new ForecastPoint(105m, 95m, 115m, 520m)]
            )
            .WithCreatedAt(baseTime.AddDays(-5));

        var latestForecast = Forecast
            .Create(
                new Id<Forecast>(Guid.NewGuid().ToString()),
                market.Id,
                symbol.Id,
                new ForecastPoint(100m, 90m, 110m, 500m),
                [new ForecastPoint(108m, 98m, 118m, 540m)]
            )
            .WithCreatedAt(baseTime);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(olderForecast);
        await _dbContext.SeedData(latestForecast);

        var query = new GetAllForecastsInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Single().Id.ShouldBe(latestForecast.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllForecastsInput(Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
