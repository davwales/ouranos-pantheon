using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.Shared;

public sealed class BacktestJobRunnerTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory =
        DbContextExtensions.MockFactory<PlutusDbContext>();
    private readonly BacktestJobRunner _runner;

    public BacktestJobRunnerTests()
    {
        _runner = new BacktestJobRunner(
            _dbContextFactory,
            Substitute.For<ILogger<BacktestJobRunner>>()
        )
        {
            MonitorInterval = TimeSpan.FromMilliseconds(20),
        };
    }

    [Fact]
    public async Task RunAsync_WhenBacktestMissing_ShouldNotInvokeRun()
    {
        // Arrange
        var invoked = false;

        // Act
        await _runner.RunAsync(
            new Id<Backtest>(Guid.NewGuid().ToString()),
            (_, _) =>
            {
                invoked = true;
                return Task.FromResult(Outcome());
            },
            CancellationToken.None
        );

        // Assert
        invoked.ShouldBeFalse();
    }

    [Theory]
    [InlineData(BacktestStatus.Running)]
    [InlineData(BacktestStatus.Completed)]
    [InlineData(BacktestStatus.Failed)]
    [InlineData(BacktestStatus.Cancelled)]
    public async Task RunAsync_WhenBacktestNotPending_ShouldSkipAndLeaveStatus(
        BacktestStatus status
    )
    {
        // Arrange
        var backtestId = await SeedBacktestAsync(status);
        var invoked = false;

        // Act
        await _runner.RunAsync(
            backtestId,
            (_, _) =>
            {
                invoked = true;
                return Task.FromResult(Outcome());
            },
            CancellationToken.None
        );

        // Assert
        invoked.ShouldBeFalse();
        (await LoadAsync(backtestId)).Status.ShouldBe(status);
    }

    [Fact]
    public async Task RunAsync_WhenRunSucceeds_ShouldCompleteWithResultsAndPositions()
    {
        // Arrange
        var backtestId = await SeedBacktestAsync();
        Backtest? claimed = null;
        var results = new BacktestResults { TotalReturn = 123m, TotalTrades = 2 };

        // Act
        await _runner.RunAsync(
            backtestId,
            (backtest, _) =>
            {
                claimed = backtest;
                return Task.FromResult(Outcome(results, 2));
            },
            CancellationToken.None
        );

        // Assert
        claimed.ShouldNotBeNull();
        claimed.Status.ShouldBe(BacktestStatus.Running);
        claimed.Strategy.Id.ShouldBe(claimed.StrategyId);

        var saved = await LoadAsync(backtestId);
        saved.Status.ShouldBe(BacktestStatus.Completed);
        saved.ProgressPercent.ShouldBe(100);
        saved.ProgressMessage.ShouldBeNull();
        saved.Results.ShouldNotBeNull();
        saved.Results.TotalReturn.ShouldBe(123m);
        saved.Positions.Count.ShouldBe(2);
        saved.Positions.Select(p => p.SymbolId).ShouldBe(["S0", "S1"], ignoreOrder: true);
    }

    [Fact]
    public async Task RunAsync_WhenRunThrows_ShouldFailWithMessage()
    {
        // Arrange
        var backtestId = await SeedBacktestAsync();

        // Act
        await _runner.RunAsync(
            backtestId,
            (_, _) => throw new InvalidOperationException("Engine exploded."),
            CancellationToken.None
        );

        // Assert
        var saved = await LoadAsync(backtestId);
        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldBe("Engine exploded.");
        saved.Results.ShouldBeNull();
    }

    [Fact]
    public async Task RunAsync_WhenUserCancelsWhileRunning_ShouldStopRunAndStayCancelled()
    {
        // Arrange
        var backtestId = await SeedBacktestAsync();
        var observedCancellation = false;

        // Act
        await _runner
            .RunAsync(
                backtestId,
                async (_, monitor) =>
                {
                    await UpdateAsync(backtestId, (_, b) => b.Cancel());
                    try
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, monitor.CancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        observedCancellation = true;
                        throw;
                    }

                    return Outcome();
                },
                CancellationToken.None
            )
            .WaitAsync(TestTimeout);

        // Assert
        observedCancellation.ShouldBeTrue();
        var saved = await LoadAsync(backtestId);
        saved.Status.ShouldBe(BacktestStatus.Cancelled);
        saved.ErrorMessage.ShouldBe("Cancelled by user.");
    }

    [Fact]
    public async Task RunAsync_WhenBacktestDeletedWhileRunning_ShouldStopRunWithoutError()
    {
        // Arrange
        var backtestId = await SeedBacktestAsync();

        // Act
        var run = () =>
            _runner
                .RunAsync(
                    backtestId,
                    async (_, monitor) =>
                    {
                        await UpdateAsync(
                            backtestId,
                            (dbContext, b) => dbContext.Backtests.Remove(b)
                        );

                        await Task.Delay(Timeout.InfiniteTimeSpan, monitor.CancellationToken);
                        return Outcome();
                    },
                    CancellationToken.None
                )
                .WaitAsync(TestTimeout);

        // Assert
        await run.ShouldNotThrowAsync();
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        (await verifyContext.Backtests.AnyAsync(b => b.Id == backtestId)).ShouldBeFalse();
    }

    [Fact]
    public async Task RunAsync_WhenCancelledBeforeOutcomeRecorded_ShouldNotOverwriteCancellation()
    {
        // Arrange
        var backtestId = await SeedBacktestAsync();

        // Act
        await _runner.RunAsync(
            backtestId,
            async (_, _) =>
            {
                await UpdateAsync(backtestId, (_, b) => b.Cancel());
                return Outcome();
            },
            CancellationToken.None
        );

        // Assert
        var saved = await LoadAsync(backtestId);
        saved.Status.ShouldBe(BacktestStatus.Cancelled);
        saved.Results.ShouldBeNull();
        saved.Positions.ShouldBeEmpty();
    }

    [Fact]
    public async Task RunAsync_WhenHostCancels_ShouldFailAsInterrupted()
    {
        // Arrange
        var backtestId = await SeedBacktestAsync();
        using var host = new CancellationTokenSource();

        // Act
        await _runner
            .RunAsync(
                backtestId,
                async (_, monitor) =>
                {
                    await host.CancelAsync();
                    await Task.Delay(Timeout.InfiniteTimeSpan, monitor.CancellationToken);
                    return Outcome();
                },
                host.Token
            )
            .WaitAsync(TestTimeout);

        // Assert
        var saved = await LoadAsync(backtestId);
        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldNotBeNull();
        saved.ErrorMessage.ShouldStartWith("Interrupted");
    }

    [Fact]
    public async Task RunAsync_WhenRunReportsProgress_ShouldPersistLatestProgress()
    {
        // Arrange
        var backtestId = await SeedBacktestAsync();
        Backtest? observedPhase = null;
        Backtest? observedReport = null;

        // Act
        await _runner
            .RunAsync(
                backtestId,
                async (_, monitor) =>
                {
                    monitor.Phase(10, 30, "Simulating...").Report(0.5);
                    observedPhase = await WaitForProgressAsync(backtestId, 20);

                    monitor.Report(70, "Almost there...");
                    observedReport = await WaitForProgressAsync(backtestId, 70);

                    return Outcome();
                },
                CancellationToken.None
            )
            .WaitAsync(TestTimeout);

        // Assert
        observedPhase.ShouldNotBeNull();
        observedPhase.ProgressMessage.ShouldBe("Simulating...");
        observedReport.ShouldNotBeNull();
        observedReport.ProgressMessage.ShouldBe("Almost there...");
        (await LoadAsync(backtestId)).Status.ShouldBe(BacktestStatus.Completed);
    }

    [Fact]
    public async Task RunAsync_WhenRunIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var run = () =>
            _runner.RunAsync(new Id<Backtest>(Guid.NewGuid().ToString()), null!, default);

        // Assert
        await run.ShouldThrowAsync<ArgumentNullException>();
    }

    private static BacktestOutcome Outcome(BacktestResults? results = null, int positions = 0)
    {
        return new BacktestOutcome(
            results ?? new BacktestResults(),
            [
                .. Enumerable
                    .Range(0, positions)
                    .Select(i => new BacktestPosition { SymbolId = $"S{i}", ProfitLoss = 1m }),
            ]
        );
    }

    private async Task<Id<Backtest>> SeedBacktestAsync(
        BacktestStatus status = BacktestStatus.Pending
    )
    {
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var strategy = Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing());

        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow,
            1000m,
            new BacktestRunSettings(),
            strategy
        );

        switch (status)
        {
            case BacktestStatus.Running:
                backtest.MarkRunning();
                break;
            case BacktestStatus.Completed:
                backtest.MarkRunning();
                backtest.Complete(new BacktestResults());
                break;
            case BacktestStatus.Failed:
                backtest.Fail("Earlier failure.");
                break;
            case BacktestStatus.Cancelled:
                backtest.Cancel();
                break;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.SeedData(strategy);
        await dbContext.SeedData(backtest);
        return backtest.Id;
    }

    private async Task UpdateAsync(
        Id<Backtest> backtestId,
        Action<PlutusDbContext, Backtest> update
    )
    {
        for (var attempt = 1; ; attempt++)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var backtest = await dbContext.Backtests.SingleAsync(b => b.Id == backtestId);
            update(dbContext, backtest);

            try
            {
                await dbContext.SaveChangesAsync();
                return;
            }
            catch (DbUpdateConcurrencyException) when (attempt < 5)
            {
                // The runner's progress monitor saved the same row concurrently.
            }
        }
    }

    private async Task<Backtest> LoadAsync(Id<Backtest> backtestId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Backtests.AsNoTracking().SingleAsync(b => b.Id == backtestId);
    }

    private async Task<Backtest?> WaitForProgressAsync(Id<Backtest> backtestId, int percent)
    {
        var deadline = DateTimeOffset.UtcNow + TestTimeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            var backtest = await LoadAsync(backtestId);
            if (backtest.ProgressPercent == percent)
            {
                return backtest;
            }

            await Task.Delay(10);
        }

        return null;
    }
}
