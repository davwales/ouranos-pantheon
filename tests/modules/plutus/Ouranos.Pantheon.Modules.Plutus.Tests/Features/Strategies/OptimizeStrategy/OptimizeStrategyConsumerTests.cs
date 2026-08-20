using System.Collections.Concurrent;
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
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
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
    private readonly ILogger<OptimizeStrategyConsumer> _logger = Substitute.For<
        ILogger<OptimizeStrategyConsumer>
    >();
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

        var executor = new StrategyExecutor(StrategyTestFactory.DefaultScorers());

        var initLogger = Substitute.For<ILogger<InitializeStep>>();
        var scoreLogger = Substitute.For<ILogger<ScoreSymbolsStep>>();

        _stepRegistry = new StepRegistry<BacktestPayload>([
            new InitializeStep(initLogger, _dataService, executor),
            new ScoreSymbolsStep(scoreLogger, new SignalScoringService([])),
            new CloseExitsStep(new SignalScoringService([])),
            new IterationSetupStep(_dbContextFactory),
            new BuyCandidatesStep(Substitute.For<ILogger<BuyCandidatesStep>>()),
            new TrackMetricsStep(),
            new LiquidateStep(),
            new ComputeResultsStep(),
        ]);

        _options = Options.Create(new OptimizationOptions());
        _backtestDataOptions = Options.Create(new BacktestDataOptions());
    }

    private OptimizeStrategyConsumer CreateConsumer()
    {
        return new(
            _logger,
            _dbContextFactory,
            _dataService,
            _options,
            _backtestDataOptions,
            _stepRegistry
        );
    }

    private static BacktestData CreateBacktestData(Market market, List<Symbol> symbols)
    {
        return BacktestData.FromRaw(market, symbols, []);
    }

    [Fact]
    public void Constructor_WhenNullLogger_ShouldThrow()
    {
        // Arrange & Act
        var act = () =>
            new OptimizeStrategyConsumer(
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
        var act = () =>
            new OptimizeStrategyConsumer(
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
        var act = () =>
            new OptimizeStrategyConsumer(
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
        var act = () =>
            new OptimizeStrategyConsumer(
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
        var act = () =>
            new OptimizeStrategyConsumer(
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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
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
        var trades = Enumerable
            .Range(0, 4)
            .Select(i =>
                Trade.Create(_fixture.Create<Id<Trade>>(), symbolId, 100m, 10m, baseTime.AddDays(i))
            )
            .ToList();

        var symbol = Symbol.Create(
            symbolId,
            "SYM",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
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

        _dataService
            .LoadDataAsync(
                marketId,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            )
            .Returns(backtestData);

        var consumer = CreateConsumer();
        var message = new OptimizeStrategyMessage(backtest.Id, Generations: 1, PopulationSize: 2);

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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
        }

        _dataService
            .LoadDataAsync(
                marketId,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            )
            .Returns(
                Task.FromException<BacktestData>(new InvalidOperationException("Simulated failure"))
            );

        var consumer = CreateConsumer();
        var message = new OptimizeStrategyMessage(backtest.Id, Generations: 1, PopulationSize: 2);

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
    public async Task Handle_WhenBacktestAlreadyClaimed_ShouldSkipProcessing()
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
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
            backtest.MarkRunning();
            await dbContext.SaveChangesAsync();
        }

        var consumer = CreateConsumer();
        var message = new OptimizeStrategyMessage(backtest.Id, Generations: 1, PopulationSize: 2);

        // Act
        await consumer.Handle(message, CancellationToken.None);

        // Assert
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var savedBacktest = await dbContext.Backtests.FirstAsync(b => b.Id == backtest.Id);
            savedBacktest.Status.ShouldBe(BacktestStatus.Running);
            savedBacktest.Results.ShouldBeNull();
        }
    }

    [Fact]
    public void NormalizeInputWeights_WhenDuplicates_ShouldKeepFirstPerKindAndOrderByKind()
    {
        // Arrange
        var inputWeights = new List<InputWeight>
        {
            new(InputKind.SignalBollingerBands, 0.11m),
            new(InputKind.SignalBollingerBands, 0.51m),
            new(InputKind.SignalRsi, 0.15m),
            new(InputKind.SignalRsi, 0.72m),
            new(InputKind.SignalPriceVelocity, 0.58m),
        };

        // Act
        var normalized = OptimizeStrategyConsumer.NormalizeInputWeights(inputWeights);

        // Assert
        normalized
            .Select(w => w.Kind)
            .ShouldBe([
                InputKind.SignalBollingerBands,
                InputKind.SignalRsi,
                InputKind.SignalPriceVelocity,
            ]);
        normalized.Single(w => w.Kind == InputKind.SignalBollingerBands).Weight.ShouldBe(0.11m);
        normalized.Single(w => w.Kind == InputKind.SignalRsi).Weight.ShouldBe(0.15m);
        normalized.Single(w => w.Kind == InputKind.SignalPriceVelocity).Weight.ShouldBe(0.58m);
    }

    [Fact]
    public void ExtractStrategyChromosome_WhenValidChromosome_ShouldReturnChromosome()
    {
        // Arrange
        var config = new TradingConfiguration
        {
            MaxPositions = 5,
            MaxPositionPercent = 0.2m,
            HoldPeriodDays = 10,
        };
        var weights = new List<InputWeight>
        {
            new(InputKind.SignalTaxAdjustedRoi, 1.0m),
            new(InputKind.SignalVolumeAnomaly, 0.5m),
            new(InputKind.SignalTrendMomentum, 0.8m),
            new(InputKind.SignalBollingerBands, 1.2m),
            new(InputKind.SignalRsi, 0.6m),
            new(InputKind.SignalMovingAverageCrossover, 0.9m),
            new(InputKind.SignalPriceVelocity, 0.7m),
        };
        var chromosome = new StrategyChromosome(config, weights, new InputThresholds());

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

    [Fact]
    public void ComputeFitness_WhenZeroTrades_AppliesUnderTradingPenalty()
    {
        // Arrange
        var results = new BacktestResults { TotalTrades = 0 };
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 10,
            PopulationSize: 20,
            SortinoWeight: 0.5,
            CagrWeight: 0.3,
            DrawdownWeight: 0.2,
            TurnoverWeight: 0.1,
            L1RegularizationWeight: 0.05,
            MinTrades: 5
        );
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var fitness = OptimizeStrategyConsumer.ComputeFitness(results, message, weights);

        // Assert
        fitness.ShouldBe(-0.55, 0.001);
    }

    [Fact]
    public void ComputeFitness_WhenTradesMeetMinTrades_AppliesNoPenalty()
    {
        // Arrange
        var results = new BacktestResults
        {
            SharpeRatio = 1.0m,
            TotalReturnPercent = 10m,
            MaxDrawdownPercent = -5m,
            TotalTrades = 5,
        };
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 10,
            PopulationSize: 20,
            SortinoWeight: 0.5,
            CagrWeight: 0.3,
            DrawdownWeight: 0.2,
            TurnoverWeight: 0.1,
            L1RegularizationWeight: 0.05,
            MinTrades: 5
        );
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var fitness = OptimizeStrategyConsumer.ComputeFitness(results, message, weights);

        // Assert
        fitness.ShouldBe(4.45, 0.001);
    }

    [Fact]
    public void ComputeFitness_WhenMinTradesZero_DisablesPenalty()
    {
        // Arrange
        var results = new BacktestResults { TotalTrades = 0 };
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 10,
            PopulationSize: 20,
            SortinoWeight: 0.5,
            CagrWeight: 0.3,
            DrawdownWeight: 0.2,
            TurnoverWeight: 0.1,
            L1RegularizationWeight: 0.05,
            MinTrades: 0
        );
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var fitness = OptimizeStrategyConsumer.ComputeFitness(results, message, weights);

        // Assert
        fitness.ShouldBe(-0.05, 0.001);
    }

    [Fact]
    public void ComputeFitness_WhenPartialShortfall_PenaltyScalesLinearly()
    {
        // Arrange
        var baseResults = new BacktestResults
        {
            SharpeRatio = 1.0m,
            TotalReturnPercent = 10m,
            MaxDrawdownPercent = -5m,
        };
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 10,
            PopulationSize: 20,
            SortinoWeight: 0.5,
            CagrWeight: 0.3,
            DrawdownWeight: 0.2,
            TurnoverWeight: 0.1,
            L1RegularizationWeight: 0.05,
            MinTrades: 5
        );
        var weights = StrategyTestFactory.DefaultWeights();

        // Act
        var fitness3 = OptimizeStrategyConsumer.ComputeFitness(
            baseResults with
            {
                TotalTrades = 3,
            },
            message,
            weights
        );
        var fitness4 = OptimizeStrategyConsumer.ComputeFitness(
            baseResults with
            {
                TotalTrades = 4,
            },
            message,
            weights
        );

        // Assert
        (fitness3 - fitness4).ShouldBe(-0.1, 0.001);
    }

    [Fact]
    public void ComputeFitness_WhenZeroTradeCandidateVsActiveLosing_ZeroTradeRanksLower()
    {
        // Arrange
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 10,
            PopulationSize: 20,
            SortinoWeight: 0.5,
            CagrWeight: 0.3,
            DrawdownWeight: 0.2,
            TurnoverWeight: 0.1,
            L1RegularizationWeight: 0.05,
            MinTrades: 5
        );
        var weights = StrategyTestFactory.DefaultWeights();

        var zeroTradeResults = new BacktestResults { TotalTrades = 0 };

        var activeLosingResults = new BacktestResults
        {
            SharpeRatio = -1m,
            TotalReturnPercent = -5m,
            MaxDrawdownPercent = -10m,
            TotalTrades = 5,
        };

        // Act
        var zeroTradeFitness = OptimizeStrategyConsumer.ComputeFitness(
            zeroTradeResults,
            message,
            weights
        );
        var activeLosingFitness = OptimizeStrategyConsumer.ComputeFitness(
            activeLosingResults,
            message,
            weights
        );

        // Assert
        zeroTradeFitness.ShouldBeLessThan(activeLosingFitness);
    }

    [Fact]
    public async Task ComputeFitnessAsync_WhenGeneSignatureAlreadyCached_ReturnsCachedValueWithoutRunningBacktest()
    {
        // Arrange
        var consumer = CreateConsumer();
        var chromosome = StrategyChromosome.CreateRandom();
        var geneSignature = string.Join(",", chromosome.Genes.Select(g => g.ToString("R")));
        var fitnessCache = new ConcurrentDictionary<string, double>();
        const double expectedCachedFitness = 42.0;
        fitnessCache[geneSignature] = expectedCachedFitness;

        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );
        var data = BacktestData.FromRaw(
            Market.Create(marketId, "Test Market", new Taxes(null)),
            [],
            []
        );
        var message = new OptimizeStrategyMessage(
            _fixture.Create<Id<Backtest>>(),
            Generations: 1,
            PopulationSize: 2
        );

        _dataService
            .LoadDataAsync(
                Arg.Any<Id<Market>>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            )
            .Returns<Task<BacktestData>>(_ => throw new InvalidOperationException("cache miss"));

        // Act
        var fitness = await consumer.ComputeFitnessAsync(
            chromosome,
            backtest,
            data,
            backtest.EndDate,
            message,
            fitnessCache,
            CancellationToken.None
        );

        // Assert
        fitness.ShouldBe(expectedCachedFitness);
        await _dataService
            .DidNotReceive()
            .LoadDataAsync(
                Arg.Any<Id<Market>>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<int>()
            );
    }
}
