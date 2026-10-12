using System.Collections.Concurrent;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Tests;

internal sealed class RecordingRunMonitor(CancellationToken cancellationToken = default)
    : IRunMonitor
{
    private readonly ConcurrentQueue<(int Percent, string Message)> _reports = new();

    public CancellationToken CancellationToken { get; } = cancellationToken;

    public IReadOnlyCollection<(int Percent, string Message)> Reports => _reports;

    public void Report(int percent, string message)
    {
        _reports.Enqueue((percent, message));
    }

    public IProgress<double> Phase(int from, int to, string message)
    {
        return new PhaseRecorder(this, from, to, message);
    }

    private sealed class PhaseRecorder(
        RecordingRunMonitor monitor,
        int from,
        int to,
        string message
    ) : IProgress<double>
    {
        public void Report(double value)
        {
            monitor.Report(from + (int)((to - from) * value), message);
        }
    }
}
