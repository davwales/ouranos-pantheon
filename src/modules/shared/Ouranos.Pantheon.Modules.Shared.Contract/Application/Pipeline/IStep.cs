namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;

public interface IStep<in TPayload>
{
    Task ExecuteAsync(PipelineContext context, TPayload payload);
}
