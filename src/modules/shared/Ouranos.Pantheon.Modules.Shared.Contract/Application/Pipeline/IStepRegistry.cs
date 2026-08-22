namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;

public interface IStepRegistry<TPayload>
{
    IStep<TPayload> Resolve<TStep>()
        where TStep : IStep<TPayload>;
}
