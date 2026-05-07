using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Pipeline;

public sealed class PipelineContextTests
{
    [Fact]
    public void Constructor_ShouldSetCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        // Act
        var context = new PipelineContext(expectedToken);

        // Assert
        context.CancellationToken.ShouldBe(expectedToken);
    }

    [Fact]
    public void Stop_WhenCalled_ShouldSetIsStopRequestedTrue()
    {
        // Arrange
        var context = new PipelineContext(CancellationToken.None);

        // Act
        context.Stop();

        // Assert
        context.IsStopRequested.ShouldBeTrue();
    }

    [Fact]
    public void CurrentIteration_ShouldBeMutable()
    {
        // Arrange
        var context = new PipelineContext(CancellationToken.None);

        // Act
        context.CurrentIteration = 42;
        var result = context.CurrentIteration;

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void TotalIterations_ShouldBeMutable()
    {
        // Arrange
        var context = new PipelineContext(CancellationToken.None);

        // Act
        context.TotalIterations = 99;
        var result = context.TotalIterations;

        // Assert
        result.ShouldBe(99);
    }
}
