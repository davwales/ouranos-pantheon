using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

public sealed class PipelineBuilder<TPayload>(IStepRegistry<TPayload> registry)
{
    private readonly List<IStep<TPayload>> _steps = [];
    private int _iterations = 1;

    public PipelineBuilder<TPayload> AddStep<TStep>()
        where TStep : IStep<TPayload>
    {
        _steps.Add(registry.Resolve<TStep>());
        return this;
    }

    public PipelineBuilder<TPayload> AddStep(IStep<TPayload> step)
    {
        Guard.Against.Null(step);
        _steps.Add(step);
        return this;
    }

    public PipelineBuilder<TPayload> AddStep(Func<PipelineContext, TPayload, Task> execute)
    {
        Guard.Against.Null(execute);
        _steps.Add(new DelegateStep<TPayload>(execute));
        return this;
    }

    public PipelineBuilder<TPayload> AddNestedPipeline(
        Func<PipelineBuilder<TPayload>, Pipeline<TPayload>> builder
    )
    {
        Guard.Against.Null(builder);
        var nestedPipeline = builder(new PipelineBuilder<TPayload>(registry));
        _steps.Add(nestedPipeline);
        return this;
    }

    public PipelineBuilder<TPayload> WithIterations(int iterations)
    {
        _iterations = iterations;
        return this;
    }

    public Pipeline<TPayload> Build()
    {
        var pipeline = new Pipeline<TPayload>(_steps);
        pipeline.Iterations = _iterations;
        return pipeline;
    }
}
