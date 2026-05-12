namespace Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

public sealed class StepRegistry<TPayload> : IStepRegistry<TPayload>
{
    private readonly Dictionary<Type, IStep<TPayload>> _steps = [];

    public StepRegistry(IEnumerable<IStep<TPayload>> steps)
    {
        foreach (var step in steps)
        {
            _steps[step.GetType()] = step;
        }
    }

    public IStep<TPayload> Resolve<TStep>()
        where TStep : IStep<TPayload>
    {
        if (_steps.TryGetValue(typeof(TStep), out var step))
        {
            return step;
        }

        throw new InvalidOperationException(
            $"No step of type '{typeof(TStep).Name}' is registered in the {nameof(StepRegistry<>)}."
        );
    }
}
