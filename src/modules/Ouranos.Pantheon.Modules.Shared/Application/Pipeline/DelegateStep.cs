namespace Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

internal sealed class DelegateStep<TPayload>(Func<PipelineContext, TPayload, Task> execute)
    : IStep<TPayload>
{
    public Task ExecuteAsync(PipelineContext context, TPayload payload)
    {
        return execute(context, payload);
    }
}
