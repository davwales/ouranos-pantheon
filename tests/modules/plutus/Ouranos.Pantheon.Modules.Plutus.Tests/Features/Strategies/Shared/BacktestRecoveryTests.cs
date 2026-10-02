using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.Shared;

public sealed class BacktestRecoveryTests
{
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;

    public BacktestRecoveryTests()
    {
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
    }

    private BacktestRecovery CreateRecovery()
    {
        return new(_dbContextFactory, Substitute.For<ILogger<BacktestRecovery>>());
    }

    private static (
        Strategy Strategy,
        Backtest RunningBacktest,
        Backtest CompletedBacktest
    ) CreateBacktests(Id<Market> marketId)
    {
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var running = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );
        running.MarkRunning();
        var completed = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );
        completed.MarkRunning();
        return (strategy, running, completed);
    }

    [Fact]
    public async Task Recover_WhenNoRunningBacktests_ShouldNotChangeAnything()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var (strategy, _, completed) = CreateBacktests(marketId);
        completed.Complete(new BacktestResults());

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(completed);
        }

        var recovery = CreateRecovery();

        // Act
        await recovery.RecoverInterruptedBacktestsAsync();

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var saved = await verifyContext
            .Backtests.AsNoTracking()
            .SingleAsync(b => b.Id == completed.Id);

        saved.Status.ShouldBe(BacktestStatus.Completed);
    }

    [Fact]
    public async Task Recover_WhenRunningBacktestsExist_ShouldMarkThemFailed()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var (strategy, running, completed) = CreateBacktests(marketId);
        completed.Complete(new BacktestResults());

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(strategy);
            await dbContext.SeedData(running);
            await dbContext.SeedData(completed);
        }

        var recovery = CreateRecovery();

        // Act
        await recovery.RecoverInterruptedBacktestsAsync();

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var savedRunning = await verifyContext
            .Backtests.AsNoTracking()
            .SingleAsync(b => b.Id == running.Id);

        savedRunning.Status.ShouldBe(BacktestStatus.Failed);
        savedRunning.ErrorMessage!.ShouldContain("Interrupted");

        var savedCompleted = await verifyContext
            .Backtests.AsNoTracking()
            .SingleAsync(b => b.Id == completed.Id);
        savedCompleted.Status.ShouldBe(BacktestStatus.Completed);
    }
}
