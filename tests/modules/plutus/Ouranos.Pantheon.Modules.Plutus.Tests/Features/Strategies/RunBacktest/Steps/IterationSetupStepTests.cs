using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class IterationSetupStepTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;

    public IterationSetupStepTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenEntityIsNull_ReturnsEarly()
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
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters) { Entity = null, ProgressInterval = 10 };

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 10, TotalIterations = 100 };

        var step = new IterationSetupStep(_dbContextFactory);

        // Act
        var execute = async () => await step.ExecuteAsync(context, payload);

        // Assert
        await execute.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenProgressIntervalIsZero_ReturnsEarly()
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
        var backtest = CreateRunningBacktest(marketId);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters) { Entity = backtest, ProgressInterval = 0 };

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 10, TotalIterations = 100 };

        var step = new IterationSetupStep(_dbContextFactory);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBe("Loading market data...");
    }

    [Fact]
    public async Task ExecuteAsync_WhenProgressIntervalIsNegative_ReturnsEarly()
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
        var backtest = CreateRunningBacktest(marketId);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters) { Entity = backtest, ProgressInterval = -1 };

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 10, TotalIterations = 100 };

        var step = new IterationSetupStep(_dbContextFactory);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBe("Loading market data...");
    }

    [Fact]
    public async Task ExecuteAsync_WhenIterationNotAtIntervalAndNotLastIteration_ReturnsEarly()
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
        var backtest = CreateRunningBacktest(marketId);
        await SeedBacktestAsync(backtest);

        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters) { Entity = backtest, ProgressInterval = 10 };

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 5, TotalIterations = 100 };

        var step = new IterationSetupStep(_dbContextFactory);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBe("Loading market data...");
    }

    [Fact]
    public async Task ExecuteAsync_WhenIterationAtInterval_UpdatesProgress()
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
        var backtest = CreateRunningBacktest(marketId);
        await SeedBacktestAsync(backtest);

        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters) { Entity = backtest, ProgressInterval = 10 };

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 10, TotalIterations = 100 };

        var step = new IterationSetupStep(_dbContextFactory);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        backtest.ProgressPercent.ShouldBe(18);
        backtest.ProgressMessage.ShouldBe("Simulating day 10 of 10...");
    }

    [Fact]
    public async Task ExecuteAsync_WhenLastIteration_UpdatesProgress()
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
        var backtest = CreateRunningBacktest(marketId);
        await SeedBacktestAsync(backtest);

        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters) { Entity = backtest, ProgressInterval = 10 };

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 99, TotalIterations = 100 };

        var step = new IterationSetupStep(_dbContextFactory);

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        backtest.ProgressPercent.ShouldBe(89);
        backtest.ProgressMessage.ShouldBe("Simulating day 99 of 10...");
    }

    [Fact]
    public async Task ExecuteAsync_WhenBacktestIsCancelled_ThrowsOperationCanceledException()
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

        var backtest = CreateRunningBacktest(marketId);
        await SeedBacktestAsync(backtest);

        await using (var cancelContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var dbBacktest = await cancelContext.Backtests.FindAsync(backtest.Id);
            dbBacktest.ShouldNotBeNull();

            dbBacktest.Cancel("Cancelled by test");
            await cancelContext.SaveChangesAsync();
        }

        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters) { Entity = backtest, ProgressInterval = 10 };

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 10, TotalIterations = 100 };

        var step = new IterationSetupStep(_dbContextFactory);

        // Act
        var act = async () => await step.ExecuteAsync(context, payload);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();

        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBe("Loading market data...");
    }

    [Fact]
    public async Task ExecuteAsync_WhenProgressUpdateTooSmall_ReturnsEarly()
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
        var backtest = CreateRunningBacktest(marketId);
        await SeedBacktestAsync(backtest);

        var parameters = new BacktestParameters(
            marketId,
            strategy,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m
        );

        var payload = new BacktestPayload(parameters) { Entity = backtest, ProgressInterval = 1 };
        var context = new PipelineContext(CancellationToken.None) { TotalIterations = 100 };
        var step = new IterationSetupStep(_dbContextFactory);


        context.CurrentIteration = 0;
        await step.ExecuteAsync(context, payload);

        backtest.ProgressPercent.ShouldBe(10);
        backtest.ProgressMessage.ShouldBe("Simulating day 0 of 10...");

        context.CurrentIteration = 1;

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        backtest.ProgressPercent.ShouldBe(10);
        backtest.ProgressMessage.ShouldBe("Simulating day 0 of 10...");
    }

    private Backtest CreateRunningBacktest(Id<Market> marketId)
    {
        var backtest = Backtest.Create(
            _fixture.Create<Id<Strategy>>(),
            marketId,
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow,
            10000m,
            strategy: null
        );
        backtest.MarkRunning();
        return backtest;
    }

    private async Task SeedBacktestAsync(Backtest backtest)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.Backtests.Add(backtest);
        await dbContext.SaveChangesAsync();
    }
}
