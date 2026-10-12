namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

/// <summary>Progress and cancellation for one running backtest or optimization.</summary>
public interface IRunMonitor
{
    /// <summary>Cancelled when the user cancels the run or the host shuts down.</summary>
    CancellationToken CancellationToken { get; }

    void Report(int percent, string message);

    /// <summary>Maps a 0-1 fraction of one phase of the run onto <paramref name="from" />-<paramref name="to" /> percent.</summary>
    IProgress<double> Phase(int from, int to, string message);
}
