using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute.ExceptionExtensions;
using Ouranos.Pantheon.Modules.Shared.WebSockets;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets;

public sealed class WebSocketWorkerTests
{
    private readonly IWebSocketClient _webSocketClient = Substitute.For<IWebSocketClient>();

    [Fact]
    public async Task ExecuteAsync_ShouldHaveExpectedLifeTime()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);
        var worker = GivenWorkerWithOptions(new WebSocketOptions());

        // Act
        await worker.StartAsync(cancellationToken);

        // Assert
        await _webSocketClient.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
        await _webSocketClient.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenWebSocketNotListening_ShouldStopApplication()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var worker = GivenWorkerWithOptions(new WebSocketOptions());

        _webSocketClient.IsListening.Returns(false);

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        await _webSocketClient.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenWebSocketClientCancels_ShouldStopApplication()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var worker = GivenWorkerWithOptions(new WebSocketOptions());
        _webSocketClient.IsListening.Throws(new OperationCanceledException());

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        await _webSocketClient.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenWebSocketThrowsException_ShouldNotStopApplication()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var worker = GivenWorkerWithOptions(
            new WebSocketOptions { HealthCheckIntervalSeconds = 0, ErrorDelayIntervalSeconds = 0 }
        );

        var listeningCalls = 0;
        _webSocketClient.IsListening.Returns(
            _ =>
            {
                listeningCalls++;
                throw new InvalidOperationException();
            },
            _ =>
            {
                listeningCalls++;
                return false;
            }
        );

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        listeningCalls.ShouldBe(2);
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