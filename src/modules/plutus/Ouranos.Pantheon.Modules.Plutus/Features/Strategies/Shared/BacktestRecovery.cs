using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

public sealed class BacktestRecovery(
    IDbContextFactory<PlutusDbContext> dbContextFactory,
    ILogger<BacktestRecovery> logger
)
{
    public async Task RecoverInterruptedBacktestsAsync(
        CancellationToken cancellationToken = default
    )
    {
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(logger);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var interrupted = await dbContext
            .Backtests.Where(b => b.Status == BacktestStatus.Running)
            .ToListAsync(cancellationToken);

        if (interrupted.Count == 0)
        {
            return;
        }

        foreach (var backtest in interrupted)
        {
            backtest.Fail("Interrupted by host restart. Restart the backtest to retry.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogWarning(
            "Recovered '{count}' backtests left in Running state by a previous host restart.",
            interrupted.Count
        );
    }
}
