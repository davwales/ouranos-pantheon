using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Utilities;

public static class Retry
{
    /// <summary>
    /// Executes <paramref name="action"/> and, on each failure, logs it and waits the next delay
    /// from <paramref name="delays"/> before retrying. When all delays are exhausted the final
    /// exception propagates to the caller. Cancellation is never retried.
    /// </summary>
    public static async Task ExecuteAsync(
        Func<CancellationToken, Task> action,
        IEnumerable<TimeSpan> delays,
        ILogger logger,
        CancellationToken cancellationToken = default
    )
    {
        Guard.Against.Null(logger);

        var attempt = 0;
        foreach (var delay in delays)
        {
            attempt++;
            try
            {
                await action(cancellationToken);
                return;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Callers retry expected conditions (e.g. data not refreshed yet), so a failed
                // attempt is not a warning; only the final one propagates.
                logger.LogInformation(
                    "Attempt {attempt} failed with '{message}'; retrying in {delay}.",
                    attempt,
                    ex.Message,
                    delay
                );
                await Task.Delay(delay, cancellationToken);
            }
        }

        await action(cancellationToken);
    }
}
