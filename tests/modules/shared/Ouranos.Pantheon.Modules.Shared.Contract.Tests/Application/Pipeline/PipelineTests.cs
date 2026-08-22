using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Pipeline;

public sealed class PipelineTests
{
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record TestPayload(int Value);

    private sealed class TrackingStep : IStep<TestPayload>
    {
        public int ExecuteCount;

        public Task ExecuteAsync(PipelineContext context, TestPayload payload)
        {
            ExecuteCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class StopStep : IStep<TestPayload>
    {
        public Task ExecuteAsync(PipelineContext context, TestPayload payload)
        {
            context.Stop();
            return Task.CompletedTask;
        }
    }

    private static Pipeline<TestPayload> CreatePipeline(IStep<TestPayload> step, int iterations = 1)
    {
        var builder = new PipelineBuilder<TestPayload>(new StepRegistry<TestPayload>([]));
        builder.AddStep(step);

        if (iterations != 1)
        {
            builder.WithIterations(iterations);
        }

        return builder.Build();
    }

    private static Pipeline<TestPayload> CreatePipeline(
        IReadOnlyList<IStep<TestPayload>> steps,
        int iterations = 1
    )
    {
        var builder = new PipelineBuilder<TestPayload>(new StepRegistry<TestPayload>([]));
        foreach (var step in steps)
        {
            builder.AddStep(step);
        }

        if (iterations != 1)
        {
            builder.WithIterations(iterations);
        }

        return builder.Build();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSingleStep_ShouldExecuteStep()
    {
        // Arrange
        var step = new TrackingStep();
        var pipeline = CreatePipeline(step);
        var context = new PipelineContext(CancellationToken.None);

        // Act
        await pipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        step.ExecuteCount.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleIterations_ShouldExecuteStepsPerIteration()
    {
        // Arrange
        var step = new TrackingStep();
        var pipeline = CreatePipeline(step, iterations: 3);
        var context = new PipelineContext(CancellationToken.None);

        // Act
        await pipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        step.ExecuteCount.ShouldBe(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStopCalledInStep_ShouldBreakOutOfCurrentIteration()
    {
        // Arrange
        var step1 = new TrackingStep();
        var step2 = new StopStep();
        var steps = new List<IStep<TestPayload>> { step1, step2 };
        var pipeline = CreatePipeline(steps, iterations: 5);
        var context = new PipelineContext(CancellationToken.None);

        // Act
        await pipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        step1.ExecuteCount.ShouldBeLessThan(5);
    }

    [Fact]
    public void ExecuteAsync_WhenCancellationTokenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var step = new TrackingStep();
        var pipeline = CreatePipeline(step);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var context = new PipelineContext(cts.Token);

        // Act & Assert
        Should.Throw<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(context, new TestPayload(0))
        );
        step.ExecuteCount.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNestedPipeline_ShouldCreateScopedContextWithIterations()
    {
        // Arrange
        var innerStep = new TrackingStep();
        var innerPipeline = CreatePipeline(innerStep, iterations: 3);

        var outerPipeline = CreatePipeline(innerPipeline, iterations: 1);
        var context = new PipelineContext(CancellationToken.None);

        // Act
        await outerPipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        innerStep.ExecuteCount.ShouldBe(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoIterations_ShouldNotExecuteSteps()
    {
        // Arrange
        var step = new TrackingStep();
        var pipeline = CreatePipeline(step, iterations: 0);
        var context = new PipelineContext(CancellationToken.None);

        // Act
        await pipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        step.ExecuteCount.ShouldBe(0);
    }
}
