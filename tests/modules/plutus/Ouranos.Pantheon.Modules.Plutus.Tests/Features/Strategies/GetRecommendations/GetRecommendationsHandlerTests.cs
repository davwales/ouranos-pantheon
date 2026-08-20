using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetRecommendationsHandler _handler;
    private readonly ILogger<GetRecommendationsHandler> _logger = Substitute.For<
        ILogger<GetRecommendationsHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetRecommendationsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();

        var executor = new StrategyExecutor(StrategyTestFactory.DefaultScorers());

        _handler = new GetRecommendationsHandler(_logger, _dbContext, executor);
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var query = new GetRecommendationsInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            10000m
        );

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenMarketNotFound_ShouldThrow()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(strategy.Id, strategy.MarketId, 10000m);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenNoSymbolsForMarket_ShouldReturnEmptyResponse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.SaveChangesAsync();

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetRecommendationsResponse>();
        result.Recommendations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenStrategyDoesNotBelongToMarket_ShouldThrow()
    {
        // Arrange
        var marketId1 = _fixture.Create<Id<Market>>();
        var marketId2 = _fixture.Create<Id<Market>>();
        var market1 = Market.Create(marketId1, "Market 1", new Taxes(null));
        var market2 = Market.Create(marketId2, "Market 2", new Taxes(null));
        await _dbContext.Markets.AddRangeAsync(market1, market2);

        var strategy = Strategy.Create(
            marketId1,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(strategy.Id, marketId2, 10000m);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetRecommendationsInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            10000m
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenSignalsAndSnapshotExist_ShouldProduceRecommendation()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.SaveChangesAsync();

        var snapshot = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            totalSpent: 1000m,
            minPrice: 5m,
            maxPrice: 15m,
            totalVolume: 100m,
            numTransactions: 10,
            limit: decimal.MaxValue,
            tax: 0m
        );
        await _dbContext.MarketTradeSnapshots.AddAsync(snapshot);
        await _dbContext.SaveChangesAsync();

        await _dbContext.LatestSignals.AddRangeAsync(
            new LatestSignal(symbolId, SignalType.BollingerBands, 0.6m),
            new LatestSignal(symbolId, SignalType.Rsi, 0.4m)
        );
        await _dbContext.SaveChangesAsync();

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            [new(InputKind.SignalBollingerBands, 1m), new(InputKind.SignalRsi, 1m)],
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetRecommendationsResponse>();
        result.Recommendations.Count.ShouldBe(1);
        var recommendation = result.Recommendations.Single();
        recommendation.SymbolId.ShouldBe(symbolId.ToString());
        recommendation.Score.ShouldBe(0.5m);
    }

    [Fact]
    public async Task Handle_WhenNegativeOrZeroBudget_ShouldThrow()
    {
        // Arrange
        var query = new GetRecommendationsInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            0m
        );

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenSignalHistoryQueryFails_FallsBackToLatestValueOnlyScoring()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.SaveChangesAsync();

        var snapshot = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            totalSpent: 1000m,
            minPrice: 5m,
            maxPrice: 15m,
            totalVolume: 100m,
            numTransactions: 10,
            limit: decimal.MaxValue,
            tax: 0m
        );
        await _dbContext.MarketTradeSnapshots.AddAsync(snapshot);
        await _dbContext.SaveChangesAsync();

        await _dbContext.LatestSignals.AddRangeAsync(
            new LatestSignal(symbolId, SignalType.BollingerBands, 0.6m),
            new LatestSignal(symbolId, SignalType.Rsi, 0.4m)
        );
        await _dbContext.SaveChangesAsync();

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            [new(InputKind.SignalBollingerBands, 1m), new(InputKind.SignalRsi, 1m)],
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recommendations.Count.ShouldBe(1);
        result.Recommendations.Single().Score.ShouldBe(0.5m);
    }

    [Fact]
    public async Task Handle_WhenNoOneHourSnapshot_ShouldFallBackToCoarserTimeframeForPrice()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.SaveChangesAsync();

        var oneDaySnapshot = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneDay,
            totalSpent: 1000m,
            minPrice: 5m,
            maxPrice: 15m,
            totalVolume: 100m,
            numTransactions: 10,
            limit: decimal.MaxValue,
            tax: 0m
        );
        await _dbContext.MarketTradeSnapshots.AddAsync(oneDaySnapshot);
        await _dbContext.SaveChangesAsync();

        await _dbContext.LatestSignals.AddRangeAsync(
            new LatestSignal(symbolId, SignalType.BollingerBands, 0.6m),
            new LatestSignal(symbolId, SignalType.Rsi, 0.4m)
        );
        await _dbContext.SaveChangesAsync();

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            [new(InputKind.SignalBollingerBands, 1m), new(InputKind.SignalRsi, 1m)],
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recommendations.Count.ShouldBe(1);
        var recommendation = result.Recommendations.Single();
        recommendation.CurrentPrice.ShouldBe(10m);
    }

    [Fact]
    public async Task Handle_WhenNoSnapshotWithPositiveTotals_ShouldReturnNoRecommendations()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.SaveChangesAsync();

        var zeroSnapshot = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            totalSpent: 0m,
            minPrice: 5m,
            maxPrice: 15m,
            totalVolume: 0m,
            numTransactions: 0,
            limit: decimal.MaxValue,
            tax: 0m
        );
        await _dbContext.MarketTradeSnapshots.AddAsync(zeroSnapshot);
        await _dbContext.SaveChangesAsync();

        await _dbContext.LatestSignals.AddRangeAsync(
            new LatestSignal(symbolId, SignalType.BollingerBands, 0.6m),
            new LatestSignal(symbolId, SignalType.Rsi, 0.4m)
        );
        await _dbContext.SaveChangesAsync();

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            [new(InputKind.SignalBollingerBands, 1m), new(InputKind.SignalRsi, 1m)],
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recommendations.Count.ShouldBe(0);
    }

    private sealed class SpyLogger<TCategory> : ILogger<TCategory>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose() { }
        }
    }
}
