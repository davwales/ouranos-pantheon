using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;
using MlForecastPoint = Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos.ForecastPoint;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.ForecastGenerator;

public sealed class ForecastGeneratorJobTests
{
    private readonly ILogger<ForecastGeneratorJob> _logger = Substitute.For<ILogger<ForecastGeneratorJob>>();
    private readonly PlutusDbContext _dbContext;
    private readonly IOuranosMachineLearningClient _mlClient = Substitute.For<IOuranosMachineLearningClient>();
    private readonly TickerFunctionContext _tickerFunctionContext;
    private readonly ForecastGeneratorJob _job;

    public ForecastGeneratorJobTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _tickerFunctionContext = new TickerFunctionContext();

        _job = new ForecastGeneratorJob(
            _logger,
            _dbContext,
            _mlClient,
            Options.Create(
                new PlutusOptions
                {
                    Forecasting = new ForecastingOptions(
                        IsEnabled: true,
                        RemoveOutdatedForecasts: false,
                        NumPredictions: 7,
                        HistoryDays: 30,
                        BatchSize: 500
                    )
                }
            )
        );
    }

    [Fact]
    public async Task Execute_WhenNoForecastableMarkets_ShouldNotCallMlClient()
    {
        // Arrange
        var market = CreateMarket(isForecastingEnabled: false);
        await _dbContext.SeedData(market);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _mlClient.DidNotReceive().GetPlutusForecasts(
            Arg.Any<GetPlutusForecastsRequest>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Execute_WhenNoTradeData_ShouldNotCallMlClient()
    {
        // Arrange
        var market = CreateMarket(isForecastingEnabled: true);
        var symbol = CreateSymbol(market.Id);
        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _mlClient.DidNotReceive().GetPlutusForecasts(
            Arg.Any<GetPlutusForecastsRequest>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Execute_WhenTradeDataExists_ShouldCallMlClientAndSaveForecasts()
    {
        // Arrange
        var market = CreateMarket(isForecastingEnabled: true);
        var symbol = CreateSymbol(market.Id);
        var trades = CreateTrades(symbol.Id);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trades);

        var mlPredictions = CreateMlPredictions(count: 7);
        _mlClient.GetPlutusForecasts(Arg.Any<GetPlutusForecastsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<List<MlForecastPoint>> { mlPredictions }));

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _mlClient.Received(1).GetPlutusForecasts(
            Arg.Any<GetPlutusForecastsRequest>(),
            Arg.Any<CancellationToken>()
        );

        _dbContext.Forecasts.Count().ShouldBe(1);

        var forecast = _dbContext.Forecasts.Single();
        forecast.MarketId.ShouldBe(market.Id);
        forecast.SymbolId.ShouldBe(symbol.Id);
    }

    [Fact]
    public async Task Execute_WhenTradeDataExists_ShouldPassCorrectSymbolCountToMlClient()
    {
        // Arrange
        var market = CreateMarket(isForecastingEnabled: true);
        var symbolA = CreateSymbol(market.Id);
        var symbolB = CreateSymbol(market.Id);
        var tradesA = CreateTrades(symbolA.Id);
        var tradesB = CreateTrades(symbolB.Id);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbolA, symbolB);
        await _dbContext.SeedData([.. tradesA, .. tradesB]);

        var mlPredictions = CreateMlPredictions(count: 7);
        _mlClient.GetPlutusForecasts(Arg.Any<GetPlutusForecastsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<List<MlForecastPoint>> { mlPredictions, mlPredictions }));

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _mlClient.Received(1).GetPlutusForecasts(
            Arg.Is<GetPlutusForecastsRequest>(r => r.Points.Count == 2 && r.NumPredictions == 7),
            Arg.Any<CancellationToken>()
        );

        _dbContext.Forecasts.Count().ShouldBe(2);
    }

    [Fact]
    public async Task Execute_WhenOnlyOneSymbolHasTradeData_ShouldOnlyForecastThatSymbol()
    {
        // Arrange
        var market = CreateMarket(isForecastingEnabled: true);
        var symbolWithTrades = CreateSymbol(market.Id);
        var symbolWithoutTrades = CreateSymbol(market.Id);
        var trades = CreateTrades(symbolWithTrades.Id);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbolWithTrades, symbolWithoutTrades);
        await _dbContext.SeedData(trades);

        var mlPredictions = CreateMlPredictions(count: 7);
        _mlClient.GetPlutusForecasts(Arg.Any<GetPlutusForecastsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<List<MlForecastPoint>> { mlPredictions }));

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        await _mlClient.Received(1).GetPlutusForecasts(
            Arg.Is<GetPlutusForecastsRequest>(r => r.Points.Count == 1),
            Arg.Any<CancellationToken>()
        );

        _dbContext.Forecasts.Count().ShouldBe(1);
        _dbContext.Forecasts.Single().SymbolId.ShouldBe(symbolWithTrades.Id);
    }

    [Fact]
    public async Task Execute_WhenExistingForecastsExist_ShouldReplaceThemWithNew()
    {
        // Arrange
        var market = CreateMarket(isForecastingEnabled: true);
        var symbol = CreateSymbol(market.Id);
        var trades = CreateTrades(symbol.Id);

        var existingForecast = Forecast.Create(
            new Id<Forecast>(Guid.NewGuid().ToString()),
            market.Id,
            symbol.Id,
            new ForecastPoint(50m, 40m, 60m, 100m),
            CreateDomainPredictions(count: 7)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trades);
        await _dbContext.SeedData(existingForecast);

        var mlPredictions = CreateMlPredictions(count: 7);
        _mlClient.GetPlutusForecasts(Arg.Any<GetPlutusForecastsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<List<MlForecastPoint>> { mlPredictions }));

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        _dbContext.Forecasts.Count().ShouldBe(1);
        _dbContext.Forecasts.Single().Id.ShouldNotBe(existingForecast.Id);
    }

    private static Market CreateMarket(bool isForecastingEnabled) =>
        Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Market",
            new Taxes(null),
            isForecastingEnabled
        );

    private static Symbol CreateSymbol(Id<Market> marketId) =>
        Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            Guid.NewGuid().ToString(),
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );

    private static Trade[] CreateTrades(Id<Symbol> symbolId) =>
    [
        .. Enumerable
            .Range(0, 30)
            .Select(i => Trade.Create(
                    new Id<Trade>(Guid.NewGuid().ToString()),
                    symbolId,
                    100m + i,
                    10m,
                    DateTimeOffset.UtcNow.AddDays(-(29 - i))
                )
            )
    ];

    private static List<MlForecastPoint> CreateMlPredictions(int count) =>
    [
        .. Enumerable.Range(0, count).Select(_ => new MlForecastPoint(100m, 90m, 110m, 50m))
    ];

    private static List<ForecastPoint> CreateDomainPredictions(int count) =>
    [
        .. Enumerable.Range(0, count).Select(_ => new ForecastPoint(100m, 90m, 110m, 50m))
    ];
}
