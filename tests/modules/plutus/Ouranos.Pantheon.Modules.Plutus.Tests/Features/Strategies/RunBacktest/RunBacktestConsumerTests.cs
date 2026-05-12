using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IBacktestDataQueryService _dataService;
    private readonly RunBacktestConsumer _consumer;
    private readonly ILogger<RunBacktestConsumer> _logger = Substitute.For<
        ILogger<RunBacktestConsumer>
    >();
    private readonly IOptions<BacktestDataOptions> _backtestDataOptions = Options.Create(
        new BacktestDataOptions()
    );

    public RunBacktestConsumerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
        _dataService = Substitute.For<IBacktestDataQueryService>();

        var executors = new List<IStrategyExecutor> { new SignalWeightedExecutor() };
        var compositeExecutor = new CompositeExecutor(executors);

        var initLogger = Substitute.For<ILogger<InitializeStep>>();
        var scoreLogger = Substitute.For<ILogger<ScoreSymbolsStep>>();

        var stepRegistry = new StepRegistry<BacktestPayload>([
            new InitializeStep(initLogger, _dataService, executors, compositeExecutor),
            new ScoreSymbolsStep(scoreLogger, []),
            new CloseExitsStep([]),
            new IterationSetupStep(_dbContextFactory),
            new BuyCandidatesStep(),
            new TrackMetricsStep(),
            new LiquidateStep(),
            new ComputeResultsStep(),
        ]);

        _consumer = new RunBacktestConsumer(
            _logger,
            _dbContextFactory,
            _dataService,
            _backtestDataOptions,
            stepRegistry
        );
    }

    private BacktestData CreateBacktestData(Market market, List<Symbol> symbols)
    {
        return BacktestData.FromRaw(market, symbols, [], [], [], []);
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
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            baseTime.AddDays(-5),
            baseTime.AddDays(-1),
            10000m,
            strategy
        );
        var trades = Enumerable
            .Range(0, 5)
            .Select(i =>
                Trade.Create(
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

        var backtestData = CreateBacktestData(market, [symbol]);

        _dataService
            .LoadDataAsync(
                marketId,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            )
            .Returns(backtestData);

        var message = new RunBacktestMessage(backtest.Id);

        // Act
        await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var saved = await verifyContext
            .Backtests.AsNoTracking()
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
            new TradingConfiguration(),
            new SignalWeightedConfig()
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
        var saved = await verifyContext
            .Backtests.AsNoTracking()
            .FirstAsync(b => b.Id == backtest.Id);

        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WhenBacktestAlreadyCompleted_ShouldSkipWithoutError()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var baseTime = DateTimeOffset.UtcNow;

        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
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

        var backtestData = CreateBacktestData(market, []);

        _dataService
            .LoadDataAsync(
                marketId,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            )
            .Returns(backtestData);

        await _consumer.Handle(new RunBacktestMessage(backtest.Id), CancellationToken.None);

        await using var verifyContext1 = await _dbContextFactory.CreateDbContextAsync();
        var saved1 = await verifyContext1
            .Backtests.AsNoTracking()
            .FirstAsync(b => b.Id == backtest.Id);
        saved1.Status.ShouldBe(BacktestStatus.Completed);

        // Act
        var secondDelivery = async () =>
            await _consumer.Handle(new RunBacktestMessage(backtest.Id), CancellationToken.None);

        // Assert
        await secondDelivery.ShouldNotThrowAsync();

        await using var verifyContext2 = await _dbContextFactory.CreateDbContextAsync();
        var saved2 = await verifyContext2
            .Backtests.AsNoTracking()
            .FirstAsync(b => b.Id == backtest.Id);
        saved2.Status.ShouldBe(BacktestStatus.Completed);
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
            new TradingConfiguration(),
            new SignalWeightedConfig()
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
        var saved = await verifyContext
            .Backtests.AsNoTracking()
            .FirstAsync(b => b.Id == backtest.Id);

        // Should NOT be Running - the catch block transitions to Failed
        saved.Status.ShouldNotBe(BacktestStatus.Running);
        saved.Status.ShouldBe(BacktestStatus.Failed);
    }
}
