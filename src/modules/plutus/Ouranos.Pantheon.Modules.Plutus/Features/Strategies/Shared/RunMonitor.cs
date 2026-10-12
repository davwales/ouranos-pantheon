using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

/// <summary>
///     Persists the latest reported progress and watches for a user cancel on a timer, so
///     simulations only report fractions and check a token instead of touching the database.
/// </summary>
internal sealed class RunMonitor : IRunMonitor, IAsyncDisposable
{
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly Id<Backtest> _backtestId;
    private readonly ILogger _logger;
    private readonly TimeSpan _interval;
    private readonly CancellationTokenSource _cancellation;
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _loop;
    private readonly Lock _gate = new();
    private (int Percent, string Message)? _latest;
    private (int Percent, string Message)? _saved;

    public RunMonitor(
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        Id<Backtest> backtestId,
        ILogger logger,
        TimeSpan interval,
        CancellationToken hostCancellation
    )
    {
        _dbContextFactory = dbContextFactory;
        _backtestId = backtestId;
        _logger = logger;
        _interval = interval;
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(hostCancellation);
        _loop = Task.Run(WatchAsync, CancellationToken.None);
    }

    public CancellationToken CancellationToken => _cancellation.Token;

    public bool IsCancelledByUser { get; private set; }

    public void Report(int percent, string message)
    {
        lock (_gate)
        {
            _latest = (Math.Clamp(percent, 0, 100), message);
        }
    }

    public IProgress<double> Phase(int from, int to, string message)
    {
        return new RunPhaseProgress(this, from, to, message);
    }

    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        await _loop;
        _stop.Dispose();
        _cancellation.Dispose();
    }

    private async Task WatchAsync()
    {
        while (!_stop.IsCancellationRequested && !_cancellation.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, _stop.Token);
                await TickAsync();
            }
            catch (OperationCanceledException) when (_stop.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not update progress for backtest '{backtestId}'.",
                    _backtestId
                );
            }
        }
    }

    private async Task TickAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(_stop.Token);

        var backtest = await dbContext.Backtests.FirstOrDefaultAsync(
            b => b.Id == _backtestId,
            _stop.Token
        );

        if (backtest is null || backtest.Status == BacktestStatus.Cancelled)
        {
            IsCancelledByUser = true;
            await _cancellation.CancelAsync();
            return;
        }

        (int Percent, string Message)? latest;
        lock (_gate)
        {
            latest = _latest;
        }

        if (latest is not { } progress || latest == _saved)
        {
            return;
        }

        backtest.UpdateProgress(progress.Percent, progress.Message);
        try
        {
            await dbContext.SaveChangesAsync(_stop.Token);
            _saved = latest;
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogDebug(
                "Backtest '{backtestId}' changed while saving progress; retrying next tick.",
                _backtestId
            );
        }
    }
}
