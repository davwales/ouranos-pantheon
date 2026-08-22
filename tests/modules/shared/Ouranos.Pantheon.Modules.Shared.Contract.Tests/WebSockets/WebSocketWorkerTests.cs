using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute.ExceptionExtensions;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets;

public sealed class WebSocketWorkerTests
{
    private readonly IWebSocketClient _webSocketClient = Substitute.For<IWebSocketClient>();

    private static readonly WebSocketOptions ZeroDelayOptions = new()
    {
        HealthCheckIntervalSeconds = 0,
        ReconnectBaseDelaySeconds = 0,
        ReconnectMaxDelaySeconds = 0,
    };

    [Fact]
    public async Task ExecuteAsync_WhenConnectionDrops_ShouldReconnect()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var worker = GivenWorkerWithOptions(ZeroDelayOptions);

        var connectCount = 0;
        _webSocketClient
            .When(x => x.ConnectAsync(Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                if (++connectCount >= 2)
                {
                    cts.Cancel();
                }
            });

        _webSocketClient.IsListening.Returns(false);

        // Act
        await worker.StartAsync(cts.Token);

        if (worker.ExecuteTask is not null)
        {
            await worker.ExecuteTask;
        }

        // Assert
        await _webSocketClient.Received(2).ConnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionDuringListen_ShouldReconnect()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var worker = GivenWorkerWithOptions(ZeroDelayOptions);

        var connectCount = 0;
        _webSocketClient
            .When(x => x.ConnectAsync(Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                if (++connectCount >= 2)
                {
                    cts.Cancel();
                }
            });

        _webSocketClient.IsListening.Throws(new InvalidOperationException("Connection error"));

        // Act
        await worker.StartAsync(cts.Token);

        if (worker.ExecuteTask is not null)
        {
            await worker.ExecuteTask;
        }

        // Assert
        await _webSocketClient.Received(2).ConnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopAndDisconnect()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var worker = GivenWorkerWithOptions(ZeroDelayOptions);

        _webSocketClient.IsListening.Returns(_ =>
        {
            cts.Cancel();
            return true;
        });

        // Act
        await worker.StartAsync(cts.Token);

        if (worker.ExecuteTask is not null)
        {
            await worker.ExecuteTask;
        }

        // Assert
        await _webSocketClient.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
        await _webSocketClient.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
    }

    private WebSocketWorker GivenWorkerWithOptions(WebSocketOptions options)
    {
        return new WebSocketWorker(
            Substitute.For<ILogger<WebSocketWorker>>(),
            _webSocketClient,
            Options.Create(options)
        );
    }
}
