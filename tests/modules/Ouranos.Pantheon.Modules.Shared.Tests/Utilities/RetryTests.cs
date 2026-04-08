using Ouranos.Pantheon.Modules.Shared.Utilities;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Utilities;

public sealed class RetryTests
{
    [Fact]
    public async Task ExecuteAsync_WhenActionSucceedsOnFirstAttempt_ShouldNotRetry()
    {
        // Arrange
        var callCount = 0;
        Task Action(CancellationToken _)
        {
            callCount++;
            return Task.CompletedTask;
        }

        // Act
        await Retry.ExecuteAsync(Action, [TimeSpan.Zero, TimeSpan.Zero]);

        // Assert
        callCount.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActionFailsOnce_ShouldRetryAndSucceed()
    {
        // Arrange
        var callCount = 0;
        Task Action(CancellationToken _)
        {
            callCount++;
            if (callCount == 1)
            {
                throw new InvalidOperationException("First attempt failed");
            }

            return Task.CompletedTask;
        }

        // Act
        await Retry.ExecuteAsync(Action, [TimeSpan.Zero]);

        // Assert
        callCount.ShouldBe(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllDelaysExhausted_ShouldThrowLastException()
    {
        // Arrange
        var callCount = 0;
        Task Action(CancellationToken _)
        {
            callCount++;
            throw new InvalidOperationException($"Attempt {callCount} failed");
        }

        // Act
        var act = async () => await Retry.ExecuteAsync(Action, [TimeSpan.Zero, TimeSpan.Zero]);

        // Assert
        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldContain("3");
        callCount.ShouldBe(3);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoDelays_ShouldExecuteExactlyOnce()
    {
        // Arrange
        var callCount = 0;
        Task Action(CancellationToken _)
        {
            callCount++;
            throw new InvalidOperationException("Always fails");
        }

        // Act
        var act = async () => await Retry.ExecuteAsync(Action, []);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
        callCount.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var ct = new CancellationToken(true);
        Task Action(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        // Act
        var act = async () => await Retry.ExecuteAsync(Action, [TimeSpan.Zero], ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
