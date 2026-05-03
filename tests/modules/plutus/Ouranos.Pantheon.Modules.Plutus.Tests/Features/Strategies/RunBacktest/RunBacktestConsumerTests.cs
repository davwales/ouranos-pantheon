using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly RunBacktestConsumer _consumer;
    private readonly ILogger<RunBacktestConsumer> _logger = Substitute.For<ILogger<RunBacktestConsumer>>();

    public RunBacktestConsumerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();

        var engineLogger = Substitute.For<ILogger<BacktestEngine>>();
        var executors = new List<IStrategyExecutor> { new SignalWeightedExecutor() };
        var compositeExecutor = new CompositeExecutor(executors);
        var engine = new BacktestEngine(engineLogger, _dbContextFactory, executors, compositeExecutor, []);

        _consumer = new RunBacktestConsumer(_logger, _dbContextFactory, engine);
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldNotThrow()
    {
        // Arrange
        var message = new RunBacktestMessage(_fixture.Create<Id<Backtest>>());

        // Act
        var run = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await run.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var message = new RunBacktestMessage(_fixture.Create<Id<Backtest>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var run = async () => await _consumer.Handle(message, cancellationToken);

        // Assert
        await run.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenBacktestFoundAndEngineSucceeds_ShouldMarkRunningThenComplete()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var baseTime = DateTimeOffset.UtcNow;

        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var symbol = Symbol.Create(symbolId, "SYM", null, "Test", marketId, new AdditionalFields());
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration { BuyThreshold = 0m, MaxPositions = 10 }
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            baseTime.AddDays(-5),
            baseTime.AddDays(-1),
            10000m,
            strategy
        );
        var trades = Enumerable.Range(0, 5)
            .Select(i => Trade.Create(
                    _fixture.Create<Id<Trade>>(),
                    symbolId,
                    100m,
                    10m,
                    baseTime.AddDays(-5 + i)
                )
            )
            .ToList();

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(market);
            await dbContext.SeedData(symbol);
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
            await dbContext.Trades.AddRangeAsync(trades);
            await dbContext.SaveChangesAsync();
        }

        var message = new RunBacktestMessage(backtest.Id);

        // Act
        await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var saved = await verifyContext.Backtests
            .AsNoTracking()
            .FirstAsync(b => b.Id == backtest.Id);

        saved.Status.ShouldBe(BacktestStatus.Completed);
        saved.Results.ShouldNotBeNull();
        saved.Results.TotalReturnPercent.ShouldBeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task Handle_WhenEngineThrowsException_ShouldFailBacktest()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var baseTime = DateTimeOffset.UtcNow;

        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.RecipeArbitrage,
            new StrategyConfiguration { MinMarginPercent = 0.01m }
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            baseTime.AddDays(-5),
            baseTime.AddDays(-1),
            10000m,
            strategy
        );

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(market);
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
            await dbContext.SaveChangesAsync();
        }

        var message = new RunBacktestMessage(backtest.Id);

        // Act
        await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var saved = await verifyContext.Backtests
            .AsNoTracking()
            .FirstAsync(b => b.Id == backtest.Id);

        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WhenEngineThrowsException_ShouldNotLeaveBacktestInRunningState()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var baseTime = DateTimeOffset.UtcNow;

        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.RecipeArbitrage,
            new StrategyConfiguration { MinMarginPercent = 0.01m }
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            baseTime.AddDays(-5),
            baseTime.AddDays(-1),
            10000m,
            strategy
        );

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(market);
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
            await dbContext.SaveChangesAsync();
        }

        var message = new RunBacktestMessage(backtest.Id);

        // Act
        await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var saved = await verifyContext.Backtests
            .AsNoTracking()
            .FirstAsync(b => b.Id == backtest.Id);

        // Should NOT be Running — the catch block transitions to Failed
        saved.Status.ShouldNotBe(BacktestStatus.Running);
        saved.Status.ShouldBe(BacktestStatus.Failed);
    }
}
