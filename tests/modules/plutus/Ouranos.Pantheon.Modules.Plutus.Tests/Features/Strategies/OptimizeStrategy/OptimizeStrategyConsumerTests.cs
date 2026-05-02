using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

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

    private static BacktestEngine CreateEngine(IDbContextFactory<PlutusDbContext> dbContextFactory)
    {
        var logger = Substitute.For<ILogger<BacktestEngine>>();
        var executors = new List<IStrategyExecutor> { new SignalWeightedExecutor() };
        var composite = new CompositeExecutor(executors);

        return new BacktestEngine(logger, dbContextFactory, executors, composite, []);
    }
}
