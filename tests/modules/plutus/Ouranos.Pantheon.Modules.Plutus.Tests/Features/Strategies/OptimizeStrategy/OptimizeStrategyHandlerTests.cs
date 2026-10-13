using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Wolverine;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly OptimizeStrategyHandler _handler;
    private readonly ILogger<OptimizeStrategyHandler> _logger = Substitute.For<
        ILogger<OptimizeStrategyHandler>
    >();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    public OptimizeStrategyHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
        _handler = new OptimizeStrategyHandler(
            _logger,
            _dbContextFactory,
            _bus,
            new StrategyEngines([
                new FakeStrategyEngine(StrategyMode.Swing),
                new FakeStrategyEngine(),
            ])
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateBacktestAndPublishOptimizeMessage()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();

        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            10,
            5,
            0.3,
            0.25,
            0.15m,
            0.08m,
            4
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OptimizeStrategyResponse>();
        result.BacktestId.ShouldNotBe(default);

        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var backtest = await verifyContext.Backtests.FindAsync(result.BacktestId);
        backtest.ShouldNotBeNull();
        backtest.Status.ShouldBe(BacktestStatus.Pending);
        backtest.Kind.ShouldBe(BacktestKind.Optimization);
        backtest.RunSettings.ShouldBe(
            new BacktestRunSettings(0.15m, 0.08m, new OptimizationSettings(10, 5, 0.3, 0.25, 4))
        );
        await _bus.Received(1)
            .PublishAsync(Arg.Is<OptimizeStrategyMessage>(m => m.BacktestId == result.BacktestId));
    }

    [Fact]
    public async Task Handle_WhenMarketFrictionOmitted_ShouldPersistDefaults()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();

        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            strategy.MarketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var backtest = await verifyContext.Backtests.FindAsync(result.BacktestId);
        backtest.ShouldNotBeNull();
        backtest.RunSettings.VolumeParticipationRate.ShouldBe(
            BacktestDataOptions.DefaultVolumeParticipationRate
        );
        backtest.RunSettings.SlippageMultiplier.ShouldBe(
            BacktestDataOptions.DefaultSlippageMultiplier
        );
        backtest.RunSettings.Optimization.ShouldBe(new OptimizationSettings(30, 50, 0.5, 0.2, 10));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    public async Task Handle_WhenOutSampleRatioOutOfRange_ShouldThrow(double ratio)
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            OutSampleRatio: ratio
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentException>();
        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(default(OptimizeStrategyMessage));
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenGenerationsExceedsMaximum_ShouldThrow()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();

        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            Generations: 501,
            PopulationSize: 10
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenPopulationSizeBelowMinimum_ShouldThrow()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();

        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            Generations: 10,
            PopulationSize: 1
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenBudgetIsZero_ShouldThrow()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            0m
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenEndDateBeforeStartDate_ShouldThrow()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var optimize = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await optimize.ShouldThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData("minPrice")]
    [InlineData("maxOpenOrders", "maxHoldHours")]
    public async Task Handle_WhenLocksAreInvalid_ShouldThrowAndNotPublish(params string[] locked)
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();

        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            new FlipConfiguration() with
            {
                LockedParameters = locked,
            }
        );

        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentException>();
        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(default(OptimizeStrategyMessage));
    }
}
