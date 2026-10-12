using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Sizing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandlerTests
{
    private readonly Fixture _fixture = new();
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

        _handler = new GetRecommendationsHandler(
            _logger,
            _dbContext,
            executor,
            Options.Create(new BacktestDataOptions())
        );
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

        var snapshot = new MarketTradeSnapshot(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            TotalSpent: 1000m,
            MinPrice: 5m,
            MaxPrice: 15m,
            TotalVolume: 100m,
            NumTransactions: 10,
            Limit: decimal.MaxValue,
            Tax: 0m
        );
        await _dbContext.MarketTradeSnapshots.AddAsync(snapshot);
        await _dbContext.SaveChangesAsync();

        await AddWeeklyVolumeAsync(marketId, symbolId, 7000m);

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

        var snapshot = new MarketTradeSnapshot(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            TotalSpent: 1000m,
            MinPrice: 5m,
            MaxPrice: 15m,
            TotalVolume: 100m,
            NumTransactions: 10,
            Limit: decimal.MaxValue,
            Tax: 0m
        );
        await _dbContext.MarketTradeSnapshots.AddAsync(snapshot);
        await _dbContext.SaveChangesAsync();

        await AddWeeklyVolumeAsync(marketId, symbolId, 7000m);

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

        var oneDaySnapshot = new MarketTradeSnapshot(
            marketId,
            symbolId,
            TimeFrame.OneDay,
            TotalSpent: 1000m,
            MinPrice: 5m,
            MaxPrice: 15m,
            TotalVolume: 100m,
            NumTransactions: 10,
            Limit: decimal.MaxValue,
            Tax: 0m
        );
        await _dbContext.MarketTradeSnapshots.AddAsync(oneDaySnapshot);
        await _dbContext.SaveChangesAsync();

        await AddWeeklyVolumeAsync(marketId, symbolId, 7000m);

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

        var zeroSnapshot = new MarketTradeSnapshot(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            TotalSpent: 0m,
            MinPrice: 5m,
            MaxPrice: 15m,
            TotalVolume: 0m,
            NumTransactions: 0,
            Limit: decimal.MaxValue,
            Tax: 0m
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

    [Fact]
    public async Task Handle_WhenSymbolIsLiquid_ShouldCapVolumeAtParticipationOfTypicalVolume()
    {
        // Arrange
        var (strategy, marketId) = await SeedScoredSymbolsAsync(
            new TradingConfiguration(),
            (Volume: 700m, Limit: null)
        );

        var query = new GetRecommendationsInput(strategy.Id, marketId, 2000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var recommendation = result.Recommendations.ShouldHaveSingleItem();
        recommendation.TypicalDailyVolume.ShouldBe(100m);
        recommendation.SuggestedVolume.ShouldBe(25m);
        recommendation.SuggestedAllocation.ShouldBe(250m);
        recommendation.LimitedBy.ShouldBe(PositionLimit.Liquidity);
    }

    [Fact]
    public async Task Handle_WhenSymbolHasBuyLimit_ShouldCapVolumeAtLimitWindows()
    {
        // Arrange
        var (strategy, marketId) = await SeedScoredSymbolsAsync(
            new TradingConfiguration(),
            (Volume: 700_000m, Limit: 100m)
        );

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var recommendation = result.Recommendations.ShouldHaveSingleItem();
        recommendation.SuggestedVolume.ShouldBe(
            100m * BacktestDataOptions.DefaultLimitWindowsPerDay
        );
        recommendation.LimitedBy.ShouldBe(PositionLimit.BuyLimit);
    }

    [Fact]
    public async Task Handle_WhenSymbolIsTooThinToFill_ShouldSkipItAndBackfillWithNext()
    {
        // Arrange
        var (strategy, marketId) = await SeedScoredSymbolsAsync(
            new TradingConfiguration(MaxPositions: 1),
            (Volume: 7m, Limit: null),
            (Volume: 700_000m, Limit: null)
        );

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var recommendation = result.Recommendations.ShouldHaveSingleItem();
        recommendation.SymbolName.ShouldBe("Symbol 1");
        recommendation.LimitedBy.ShouldBe(PositionLimit.Budget);
    }

    [Fact]
    public async Task Handle_WhenSeveralSymbolsQualify_ShouldNotAllocateMoreThanBudget()
    {
        // Arrange
        var (strategy, marketId) = await SeedScoredSymbolsAsync(
            new TradingConfiguration(MaxPositionPercent: 0.4m),
            (Volume: 700_000m, Limit: null),
            (Volume: 700_000m, Limit: null),
            (Volume: 700_000m, Limit: null)
        );

        var query = new GetRecommendationsInput(strategy.Id, marketId, 10000m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recommendations.Count.ShouldBe(3);
        result.Recommendations.Sum(r => r.SuggestedAllocation).ShouldBe(10000m);
        result.Recommendations.Max(r => r.SuggestedAllocation).ShouldBe(4000m);
    }

    private async Task AddWeeklyVolumeAsync(
        Id<Market> marketId,
        Id<Symbol> symbolId,
        decimal volume
    )
    {
        await _dbContext.MarketTradeSnapshots.AddAsync(
            new MarketTradeSnapshot(
                marketId,
                symbolId,
                TimeFrame.OneWeek,
                TotalSpent: volume * 10m,
                MinPrice: 10m,
                MaxPrice: 10m,
                TotalVolume: volume,
                NumTransactions: 10,
                Limit: decimal.MaxValue,
                Tax: 0m
            )
        );

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Seeds one symbol per entry priced at 10 with a weekly volume and optional buy limit,
    ///     all scoring identically, with signal strength descending in seed order.
    /// </summary>
    private async Task<(Strategy Strategy, Id<Market> MarketId)> SeedScoredSymbolsAsync(
        TradingConfiguration configuration,
        params (decimal Volume, decimal? Limit)[] symbols
    )
    {
        var marketId = _fixture.Create<Id<Market>>();
        await _dbContext.Markets.AddAsync(Market.Create(marketId, "Test Market", new Taxes(null)));

        for (var i = 0; i < symbols.Length; i++)
        {
            var symbolId = _fixture.Create<Id<Symbol>>();

            await _dbContext.Symbols.AddAsync(
                Symbol.Create(
                    symbolId,
                    $"SYM{i}",
                    null,
                    $"Symbol {i}",
                    marketId,
                    new AdditionalFields(Limit: symbols[i].Limit)
                )
            );

            await _dbContext.MarketTradeSnapshots.AddAsync(
                new MarketTradeSnapshot(
                    marketId,
                    symbolId,
                    TimeFrame.OneHour,
                    TotalSpent: 1000m,
                    MinPrice: 10m,
                    MaxPrice: 10m,
                    TotalVolume: 100m,
                    NumTransactions: 10,
                    Limit: decimal.MaxValue,
                    Tax: 0m
                )
            );

            await AddWeeklyVolumeAsync(marketId, symbolId, symbols[i].Volume);

            await _dbContext.LatestSignals.AddAsync(
                new LatestSignal(symbolId, SignalType.Rsi, 0.9m - (i * 0.1m))
            );
        }

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            configuration,
            [new(InputKind.SignalRsi, 1m)],
            null
        );

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        return (strategy, marketId);
    }
}
