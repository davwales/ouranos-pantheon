using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyConsumerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<OptimizeStrategyConsumer> _logger = Substitute.For<ILogger<OptimizeStrategyConsumer>>();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IBacktestDataQueryService _dataService;
    private readonly IOptions<OptimizationOptions> _options;
    private readonly IOptions<BacktestDataOptions> _backtestDataOptions;
    private readonly IStepRegistry<BacktestPayload> _stepRegistry;

    public OptimizeStrategyConsumerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
        _dataService = Substitute.For<IBacktestDataQueryService>();

        var executors = new List<IStrategyExecutor> { new SignalWeightedExecutor() };
        var compositeExecutor = new CompositeExecutor(executors);

        var initLogger = Substitute.For<ILogger<InitializeStep>>();
        var scoreLogger = Substitute.For<ILogger<ScoreSymbolsStep>>();

        _stepRegistry = new StepRegistry<BacktestPayload>(
            [
                new InitializeStep(initLogger, _dataService, executors, compositeExecutor),
                new ScoreSymbolsStep(scoreLogger, []),
                new CloseExitsStep([]),
                new IterationSetupStep(_dbContextFactory),
                new BuyCandidatesStep(),
                new TrackMetricsStep(),
                new LiquidateStep(),
                new ComputeResultsStep(),
            ]
        );

        _options = Options.Create(new OptimizationOptions());
        _backtestDataOptions = Options.Create(new BacktestDataOptions());
    }

    private OptimizeStrategyConsumer CreateConsumer() => new(
        _logger,
        _dbContextFactory,
        _dataService,
        _options,
        _backtestDataOptions,
        _stepRegistry
    );

    private BacktestData CreateBacktestData(Market market, List<Symbol> symbols)
    {
        return BacktestData.FromRaw(
            market,
            symbols,
            [],
            [],
            [],
            []
        );
    }

    [Fact]
    public void Constructor_WhenNullLogger_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(
            null!,
            _dbContextFactory,
            _dataService,
            _options,
            _backtestDataOptions,
            _stepRegistry
        );

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullDbContextFactory_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(
            _logger,
            null!,
            _dataService,
            _options,
            _backtestDataOptions,
            _stepRegistry
        );

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullDataService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(
            _logger,
            _dbContextFactory,
            null!,
            _options,
            _backtestDataOptions,
            _stepRegistry
        );

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullOptions_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(
            _logger,
            _dbContextFactory,
            _dataService,
            null!,
            _backtestDataOptions,
            _stepRegistry
        );

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullBacktestDataOptions_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(
            _logger,
            _dbContextFactory,
            _dataService,
            _options,
            null!,
            _stepRegistry
        );

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldReturnWithoutError()
    {
        // Arrange
        var consumer = CreateConsumer();
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 10,
            PopulationSize: 10
        );

        // Act
        var act = async () => await consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var consumer = CreateConsumer();
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 10,
            PopulationSize: 10
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var act = async () => await consumer.Handle(message, cancellationToken);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenBacktestFound_ShouldMarkRunningThenComplete()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
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
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );

        var baseTime = DateTimeOffset.UtcNow.AddDays(-5);
        var trades = Enumerable.Range(0, 4)
            .Select(i => Trade.Create(
                    _fixture.Create<Id<Trade>>(),
                    symbolId,
                    100m,
                    10m,
                    baseTime.AddDays(i)
                )
            )
            .ToList();

        var symbol = Symbol.Create(symbolId, "SYM", null, "Test Symbol", marketId, new AdditionalFields());
        var market = Market.Create(marketId, "Test Market", new Taxes(null));

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

        _dataService.LoadDataAsync(
                marketId,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            )
            .Returns(backtestData);

        var consumer = CreateConsumer();
        var message = new OptimizeStrategyMessage(
            backtest.Id,
            Generations: 1,
            PopulationSize: 2
        );

        // Act
        await consumer.Handle(message, CancellationToken.None);

        // Assert
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var savedBacktest = await dbContext.Backtests.FirstAsync(b => b.Id == backtest.Id);
            savedBacktest.Status.ShouldBe(BacktestStatus.Completed);
            savedBacktest.Results.ShouldNotBeNull();
            savedBacktest.ErrorMessage.ShouldBeNull();
        }
    }

    [Fact]
    public async Task Handle_WhenOptimizationFails_ShouldMarkBacktestAsFailed()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
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
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );

        // Seed strategy and backtest, but NOT the market - LoadDataAsync will fail
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
        }

        _dataService.LoadDataAsync(
                marketId,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            )
            .Returns(Task.FromException<BacktestData>(new InvalidOperationException("Simulated failure")));

        var consumer = CreateConsumer();
        var message = new OptimizeStrategyMessage(
            backtest.Id,
            Generations: 1,
            PopulationSize: 2
        );

        // Act
        await consumer.Handle(message, CancellationToken.None);

        // Assert
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var savedBacktest = await dbContext.Backtests.FirstAsync(b => b.Id == backtest.Id);
            savedBacktest.Status.ShouldBe(BacktestStatus.Failed);
            savedBacktest.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
            savedBacktest.Results.ShouldBeNull();
        }
    }

    [Fact]
    public void ExtractStrategyChromosome_WhenValidChromosome_ShouldReturnChromosome()
    {
        // Arrange
        var config = new TradingConfiguration { MaxPositions = 5, MaxPositionPercent = 0.2m, HoldPeriodDays = 10 };
        var weights = new SignalWeightedConfig(
            BuyThreshold: 50m,
            SellThreshold: 50m,
            TaxAdjustedRoiWeight: 1.0m,
            VolumeAnomalyWeight: 0.5m,
            TrendMomentumWeight: 0.8m,
            BollingerBandsWeight: 1.2m,
            RsiWeight: 0.6m,
            MovingAverageCrossoverWeight: 0.9m,
            PriceVelocityWeight: 0.7m
        );
        var chromosome = new SignalWeightedChromosome(config, weights);

        // Act
        var result = OptimizeStrategyConsumer.ExtractStrategyChromosome(chromosome);

        // Assert
        result.ShouldBeSameAs(chromosome);
        result.Configuration.MaxPositions.ShouldBe(5);
        result.Configuration.MaxPositionPercent.ShouldBe(0.2m);
        result.Configuration.HoldPeriodDays.ShouldBe(10);
    }

    [Fact]
    public void ExtractStrategyChromosome_WhenWrongType_ShouldThrow()
    {
        // Arrange
        var wrongChromosome = Substitute.For<IChromosome<double>>();

        // Act
        var act = () => OptimizeStrategyConsumer.ExtractStrategyChromosome(wrongChromosome);

        // Assert
        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("StrategyChromosome");
    }
}
