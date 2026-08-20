using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetBacktest;

public sealed class GetBacktestHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetBacktestHandler _handler;
    private readonly ILogger<GetBacktestHandler> _logger = Substitute.For<
        ILogger<GetBacktestHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetBacktestHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetBacktestHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnBacktest()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
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

        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var query = new GetBacktestInput(backtest.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(backtest.Id);
        result.StrategyId.ShouldBe(strategy.Id);
        result.MarketId.ShouldBe(marketId);
        result.Status.ShouldBe(BacktestStatus.Pending);
        result.Kind.ShouldBe(BacktestKind.Backtest);
        result.Budget.ShouldBe(10000m);
        result.ProgressPercent.ShouldBe(0);
        result.ProgressMessage.ShouldBeNull();
        result.Results.ShouldBeNull();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCompletedBacktest_ShouldReturnPositions()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
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

        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        backtest.MarkRunning();

        var positions = new List<BacktestPosition>
        {
            new(
                "BTCUSD",
                "Bitcoin",
                50000m,
                52000m,
                0.5m,
                1000m,
                2.0m,
                DateTimeOffset.UtcNow.AddDays(-5),
                DateTimeOffset.UtcNow.AddDays(-1)
            ),
            new(
                "ETHUSD",
                "Ethereum",
                3000m,
                3200m,
                2.0m,
                400m,
                6.67m,
                DateTimeOffset.UtcNow.AddDays(-4),
                DateTimeOffset.UtcNow.AddDays(-2)
            ),
        };

        var results = new BacktestResults(
            TotalReturn: 1000m,
            TotalReturnPercent: 10.0m,
            MaxDrawdown: 200m,
            MaxDrawdownPercent: 2.0m,
            WinRate: 60.0m,
            TotalTrades: 10,
            WinningTrades: 6,
            LosingTrades: 4,
            SharpeRatio: 1.5m,
            SortinoRatio: 2.0m,
            CalmarRatio: 1.2m,
            Cagr: 8.5m,
            ProfitFactor: 2.0m,
            Expectancy: 100m,
            AverageTradeReturn: 100m,
            BestTrade: 500m,
            WorstTrade: -200m,
            FinalBalance: 11000m,
            TurnoverRate: 0m,
            IsValidated: false,
            OutSampleResults: null,
            OptimizedInputWeights: null,
            OptimizedThresholds: null,
            OptimizedConfiguration: null
        );

        backtest.Complete(results);
        backtest.Positions = positions;
        await _dbContext.SaveChangesAsync();

        var query = new GetBacktestInput(backtest.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(backtest.Id);
        result.Status.ShouldBe(BacktestStatus.Completed);
        result.Results.ShouldNotBeNull();
        result.Positions.Count.ShouldBe(2);

        var btcPosition = result.Positions.First(p => p.SymbolId == "BTCUSD");
        btcPosition.SymbolName.ShouldBe("Bitcoin");
        btcPosition.EntryPrice.ShouldBe(50000m);
        btcPosition.ExitPrice.ShouldBe(52000m);

        var ethPosition = result.Positions.First(p => p.SymbolId == "ETHUSD");
        ethPosition.SymbolName.ShouldBe("Ethereum");
        ethPosition.EntryPrice.ShouldBe(3000m);
        ethPosition.ExitPrice.ShouldBe(3200m);
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldThrow()
    {
        // Arrange
        var query = new GetBacktestInput(_fixture.Create<Id<Backtest>>());

        // Act
        var run = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await run.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetBacktestInput(_fixture.Create<Id<Backtest>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var run = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await run.ShouldThrowAsync<OperationCanceledException>();
    }
}
