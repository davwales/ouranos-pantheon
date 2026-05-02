using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyConsumerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<OptimizeStrategyConsumer> _logger = Substitute.For<ILogger<OptimizeStrategyConsumer>>();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly BacktestEngine _engine;
    private readonly IOptions<OptimizationOptions> _options;

    public OptimizeStrategyConsumerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
        _engine = CreateEngine(_dbContextFactory);
        _options = Options.Create(new OptimizationOptions());
    }

    [Fact]
    public void Constructor_WhenNullLogger_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(null!, _dbContextFactory, _engine, _options);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullDbContextFactory_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(_logger, null!, _engine, _options);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullEngine_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(_logger, _dbContextFactory, null!, _options);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenNullOptions_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new OptimizeStrategyConsumer(_logger, _dbContextFactory, _engine, null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldReturnWithoutError()
    {
        // Arrange
        var consumer = new OptimizeStrategyConsumer(_logger, _dbContextFactory, _engine, _options);
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
        var consumer = new OptimizeStrategyConsumer(_logger, _dbContextFactory, _engine, _options);
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
            new StrategyConfiguration { BuyThreshold = 0m, MaxPositions = 10 }
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

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var market = Market.Create(marketId, "Test Market", new Taxes(null));
            await dbContext.SeedData(market);
            await dbContext.SeedData(symbol);
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
            await dbContext.Trades.AddRangeAsync(trades);
            await dbContext.SaveChangesAsync();
        }

        var consumer = new OptimizeStrategyConsumer(_logger, _dbContextFactory, _engine, _options);
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
            new StrategyConfiguration { BuyThreshold = 0.1m }
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );

        // Seed strategy and backtest, but NOT the market — LoadDataAsync will fail
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(backtest);
        }

        var consumer = new OptimizeStrategyConsumer(_logger, _dbContextFactory, _engine, _options);
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
    public void ExtractConfiguration_WhenValidChromosome_ShouldReturnConfig()
    {
        // Arrange
        var chromosome = new StrategyConfigurationChromosome(
            StrategyType.SignalWeighted,
            new StrategyConfiguration { MaxPositions = 5, MaxPositionPercent = 0.2m }
        );

        var method = typeof(OptimizeStrategyConsumer).GetMethod(
            "ExtractConfiguration",
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            [typeof(IChromosome<double>)],
            null
        );
        method.ShouldNotBeNull();

        // Act
        var result = (StrategyConfiguration)method.Invoke(null, [chromosome])!;

        // Assert
        result.ShouldNotBeNull();
        result.MaxPositions.ShouldBe(5);
        result.MaxPositionPercent.ShouldBe(0.2m);
    }

    [Fact]
    public void ExtractConfiguration_WhenWrongType_ShouldThrow()
    {
        // Arrange
        var wrongChromosome = Substitute.For<IChromosome<double>>();

        var method = typeof(OptimizeStrategyConsumer).GetMethod(
            "ExtractConfiguration",
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            [typeof(IChromosome<double>)],
            null
        );
        method.ShouldNotBeNull();

        // Act
        var act = () => method.Invoke(null, [wrongChromosome]);

        // Assert
        var exception = act.ShouldThrow<TargetInvocationException>();
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
        exception.InnerException!.Message.ShouldContain("StrategyConfigurationChromosome");
    }

    private static BacktestEngine CreateEngine(IDbContextFactory<PlutusDbContext> dbContextFactory)
    {
        var logger = Substitute.For<ILogger<BacktestEngine>>();
        var executors = new List<IStrategyExecutor> { new SignalWeightedExecutor() };
        var composite = new CompositeExecutor(executors);

        return new BacktestEngine(logger, dbContextFactory, executors, composite, []);
    }
}
