using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Pipeline;

public sealed class PipelineBuilderTests
{
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record TestPayload(int Value);

    private sealed class IncrementStep : IStep<TestPayload>
    {
        public int ExecuteCount;

        public Task ExecuteAsync(PipelineContext context, TestPayload payload)
        {
            ExecuteCount++;
            return Task.CompletedTask;
        }
    }

    private static IStepRegistry<TestPayload> CreateEmptyRegistry()
    {
        return new StepRegistry<TestPayload>([]);
    }

    [Fact]
    public async Task Build_WhenNoSteps_ShouldReturnEmptyPipeline()
    {
        // Arrange
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());

        // Act
        var pipeline = builder.Build();
        var context = new PipelineContext(CancellationToken.None);
        var payload = new TestPayload(0);

        // Assert
        pipeline.ShouldNotBeNull();

        var execute = async () => await pipeline.ExecuteAsync(context, payload);
        await execute.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task AddStep_WhenInstanceStep_ShouldExecuteStep()
    {
        // Arrange
        var step = new IncrementStep();
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());
        builder.AddStep(step);

        // Act
        var pipeline = builder.Build();
        var context = new PipelineContext(CancellationToken.None);
        await pipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        step.ExecuteCount.ShouldBe(1);
    }

    [Fact]
    public async Task AddStep_WhenDelegateStep_ShouldExecuteDelegate()
    {
        // Arrange
        var executed = false;
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());
        builder.AddStep(
            (_, _) =>
            {
                executed = true;
                return Task.CompletedTask;
            }
        );

        // Act
        var pipeline = builder.Build();
        var context = new PipelineContext(CancellationToken.None);
        await pipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        executed.ShouldBeTrue();
    }

    [Fact]
    public async Task Build_WhenMultipleSteps_ShouldExecuteInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());

        builder
            .AddStep(
                (_, _) =>
                {
                    executionOrder.Add(1);
                    return Task.CompletedTask;
                }
            )
            .AddStep(
                (_, _) =>
                {
                    executionOrder.Add(2);
                    return Task.CompletedTask;
                }
            )
            .AddStep(
                (_, _) =>
                {
                    executionOrder.Add(3);
                    return Task.CompletedTask;
                }
            );

        // Act
        var pipeline = builder.Build();
        var context = new PipelineContext(CancellationToken.None);
        await pipeline.ExecuteAsync(context, new TestPayload(0));

        // Assert
        executionOrder.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void AddStep_WhenNullInstance_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());

        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
            builder.AddStep((IStep<TestPayload>)null!)
        );

        // Assert
        exception.ParamName.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void WithIterations_WhenCalledBeforeBuild_ShouldSetIterationsOnPipeline()
    {
        // Arrange
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());
        builder.WithIterations(5);

        // Act
        var pipeline = builder.Build();

        // Assert
        pipeline.Iterations.ShouldBe(5);
    }

    [Fact]
    public void WithIterations_WhenNotCalled_ShouldDefaultToOne()
    {
        // Arrange
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());

        // Act
        var pipeline = builder.Build();

        // Assert
        pipeline.Iterations.ShouldBe(1);
    }

    [Fact]
    public void WithIterations_WhenCalledMultipleTimes_ShouldUseLastValue()
    {
        // Arrange
        var builder = new PipelineBuilder<TestPayload>(CreateEmptyRegistry());
        builder.WithIterations(3).WithIterations(10);

        // Act
        var pipeline = builder.Build();

        // Assert
        pipeline.Iterations.ShouldBe(10);
    }
}
