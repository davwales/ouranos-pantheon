namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

internal sealed class RunPhaseProgress(IRunMonitor monitor, int from, int to, string message)
    : IProgress<double>
{
    public void Report(double value)
    {
        monitor.Report(from + (int)((to - from) * Math.Clamp(value, 0, 1)), message);
    }
}
