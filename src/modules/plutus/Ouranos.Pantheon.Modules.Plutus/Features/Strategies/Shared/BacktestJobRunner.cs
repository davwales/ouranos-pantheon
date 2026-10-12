using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

/// <summary>
///     The only place a queued backtest or optimization changes status: it claims the run,
///     executes it under a <see cref="IRunMonitor" />, and records the outcome. A run the user
///     cancelled is left cancelled; anything else that stops it early marks it failed.
/// </summary>
public sealed class BacktestJobRunner(
    IDbContextFactory<PlutusDbContext> dbContextFactory,
    ILogger<BacktestJobRunner> logger
)
{
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory = Guard.Against.Null(
        dbContextFactory
    );
    private readonly ILogger<BacktestJobRunner> _logger = Guard.Against.Null(logger);

    public TimeSpan MonitorInterval { get; init; } = TimeSpan.FromSeconds(2);

    public async Task RunAsync(
        Id<Backtest> backtestId,
        Func<Backtest, IRunMonitor, Task<BacktestOutcome>> run,
        CancellationToken cancellationToken
    )
    {
        Guard.Against.Null(run);

        var backtest = await ClaimAsync(backtestId, cancellationToken);
        if (backtest is null)
        {
            return;
        }

        BacktestOutcome? outcome = null;
        string? failure = null;

        var monitor = new RunMonitor(
            _dbContextFactory,
            backtestId,
            _logger,
            MonitorInterval,
            cancellationToken
        );

        try
        {
            monitor.Report(1, "Loading market data...");
            outcome = await run(backtest, monitor);
        }
        catch (OperationCanceledException) when (monitor.IsCancelledByUser)
        {
            _logger.LogInformation(
                "Backtest '{backtestId}' was cancelled by the user.",
                backtestId
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Backtest '{backtestId}' was interrupted by host shutdown or timeout.",
                backtestId
            );
            failure = "Interrupted by host shutdown or timeout. Restart the backtest to retry.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backtest '{backtestId}' failed.", backtestId);
            failure = ex.Message;
        }
        finally
        {
            await monitor.DisposeAsync();
        }

        if (outcome is not null)
        {
            await FinishAsync(
                backtestId,
                b =>
                {
                    b.Complete(outcome.Results);
                    b.Positions = [.. outcome.Positions];
                }
            );
            _logger.LogDebug("Backtest '{backtestId}' completed.", backtestId);
        }
        else if (failure is not null)
        {
            await FinishAsync(backtestId, b => b.Fail(failure));
        }
    }

    private async Task<Backtest?> ClaimAsync(
        Id<Backtest> backtestId,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var backtest = await dbContext
            .Backtests.Include(b => b.Strategy)
            .FirstOrDefaultAsync(b => b.Id == backtestId, cancellationToken);

        if (backtest is null)
        {
            _logger.LogWarning("Backtest '{backtestId}' not found.", backtestId);
            return null;
        }

        if (backtest.Status != BacktestStatus.Pending)
        {
            _logger.LogWarning(
                "Backtest '{backtestId}' is already {status}. Skipping duplicate delivery.",
                backtestId,
                backtest.Status
            );
            return null;
        }

        backtest.MarkRunning();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return backtest;
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Backtest '{backtestId}' was claimed by a concurrent delivery. Skipping.",
                backtestId
            );
            return null;
        }
    }

    /// <summary>Applies a terminal transition unless the run stopped being Running meanwhile.</summary>
    private async Task FinishAsync(Id<Backtest> backtestId, Action<Backtest> finish)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
            CancellationToken.None
        );

        var backtest = await dbContext.Backtests.FirstOrDefaultAsync(b => b.Id == backtestId);

        if (backtest is not { Status: BacktestStatus.Running })
        {
            _logger.LogInformation(
                "Backtest '{backtestId}' is {status}; leaving it unchanged.",
                backtestId,
                backtest?.Status
            );
            return;
        }

        finish(backtest);
        try
        {
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Backtest '{backtestId}' changed while recording its outcome; leaving it unchanged.",
                backtestId
            );
        }
    }
}
