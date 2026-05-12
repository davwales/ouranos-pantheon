namespace Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

public interface IStepRegistry<TPayload>
{
    IStep<TPayload> Resolve<TStep>()
        where TStep : IStep<TPayload>;
}
