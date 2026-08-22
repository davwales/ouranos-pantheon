namespace Ouranos.Pantheon.Modules.Shared.Contract.Utilities;

public static class Retry
{
    /// <summary>
    /// Executes <paramref name="action"/> and, on each failure, waits the next delay from
    /// <paramref name="delays"/> before retrying. When all delays are exhausted the final
    /// exception propagates to the caller.
    /// </summary>
    public static async Task ExecuteAsync(
        Func<CancellationToken, Task> action,
        IEnumerable<TimeSpan> delays,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var delay in delays)
        {
            try
            {
                await action(cancellationToken);
                return;
            }
            catch
            {
                await Task.Delay(delay, cancellationToken);
            }
        }

        await action(cancellationToken);
    }
}
