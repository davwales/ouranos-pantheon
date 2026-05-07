namespace Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

public sealed class Pipeline<TPayload> : IStep<TPayload>
{
    private readonly IReadOnlyList<IStep<TPayload>> _steps;

    public int Iterations { get; internal set; } = 1;

    internal Pipeline(IReadOnlyList<IStep<TPayload>> steps)
    {
        _steps = steps;
    }

    public async Task ExecuteAsync(PipelineContext context, TPayload payload)
    {
        var scopedContext = new PipelineContext(context.CancellationToken) { TotalIterations = Iterations };

        for (var i = 0; i < Iterations; i++)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (scopedContext.IsStopRequested)
            {
                break;
            }

            scopedContext.CurrentIteration = i;

            foreach (var step in _steps)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await step.ExecuteAsync(scopedContext, payload);

                if (scopedContext.IsStopRequested)
                {
                    break;
                }
            }
        }
    }
}