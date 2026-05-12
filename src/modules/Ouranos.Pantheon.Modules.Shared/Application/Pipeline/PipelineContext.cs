namespace Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

public sealed class PipelineContext(CancellationToken cancellationToken)
{
    public int CurrentIteration { get; set; }
    public int TotalIterations { get; set; }
    public bool IsStopRequested { get; private set; }
    public CancellationToken CancellationToken { get; } = cancellationToken;

    public void Stop()
    {
        IsStopRequested = true;
    }
}
