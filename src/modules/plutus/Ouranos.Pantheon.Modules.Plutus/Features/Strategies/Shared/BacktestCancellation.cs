using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

public static class BacktestCancellation
{
    public static async Task FinalizeAsync(
        Backtest backtest,
        PlutusDbContext dbContext,
        ILogger logger
    )
    {
        Guard.Against.Null(backtest);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(logger);

        var currentStatus = await dbContext
            .Backtests.AsNoTracking()
            .Where(b => b.Id == backtest.Id)
            .Select(b => (BacktestStatus?)b.Status)
            .FirstOrDefaultAsync(CancellationToken.None);

        if (currentStatus is null)
        {
            logger.LogDebug("Backtest '{backtestId}' no longer exists.", backtest.Id);
            return;
        }

        if (currentStatus == BacktestStatus.Cancelled)
        {
            logger.LogInformation(
                "Backtest '{backtestId}' was cancelled by the user.",
                backtest.Id
            );
            return;
        }

        logger.LogInformation(
            "Backtest '{backtestId}' was interrupted by host shutdown or timeout.",
            backtest.Id
        );

        if (backtest.Status is BacktestStatus.Pending or BacktestStatus.Running)
        {
            backtest.Fail(
                "Interrupted by host shutdown or timeout. Restart the backtest to retry."
            );

            try
            {
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogWarning(
                    "Backtest '{backtestId}' was modified concurrently during finalization.",
                    backtest.Id
                );
            }
        }
    }
}
