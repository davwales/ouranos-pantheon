using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Utilities;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Utilities;

public sealed class RetryTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();

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
        await Retry.ExecuteAsync(Action, [TimeSpan.Zero, TimeSpan.Zero], _logger);

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
        await Retry.ExecuteAsync(Action, [TimeSpan.Zero], _logger);

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
        var act = async () =>
            await Retry.ExecuteAsync(Action, [TimeSpan.Zero, TimeSpan.Zero], _logger);

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
        var act = async () => await Retry.ExecuteAsync(Action, [], _logger);

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
        var act = async () => await Retry.ExecuteAsync(Action, [TimeSpan.Zero], _logger, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAttemptFails_ShouldLogIt()
    {
        // Arrange
        var callCount = 0;
        Task Action(CancellationToken _)
        {
            callCount++;
            if (callCount == 1)
            {
                throw new InvalidOperationException("Not ready");
            }

            return Task.CompletedTask;
        }

        // Act
        await Retry.ExecuteAsync(Action, [TimeSpan.Zero], _logger);

        // Assert
        _logger
            .ReceivedCalls()
            .Count(call =>
                call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel?)call.GetArguments()[0] == LogLevel.Information
            )
            .ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelledDuringAnAttempt_ShouldNotRetry()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var callCount = 0;
        Task Action(CancellationToken token)
        {
            callCount++;
            cts.Cancel();
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        // Act
        var act = async () =>
            await Retry.ExecuteAsync(Action, [TimeSpan.Zero, TimeSpan.Zero], _logger, cts.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
        callCount.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () =>
            await Retry.ExecuteAsync(_ => Task.CompletedTask, [TimeSpan.Zero], null!);

        // Assert
        await act.ShouldThrowAsync<ArgumentNullException>();
    }
}
