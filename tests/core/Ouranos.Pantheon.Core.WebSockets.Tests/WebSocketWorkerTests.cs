using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute.ExceptionExtensions;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Tests;

public sealed class WebSocketWorkerTests
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWebSocketClient _webSocketClient;

    public WebSocketWorkerTests()
    {
        _webSocketClient = Substitute.For<IWebSocketClient>();
        _applicationLifetime = Substitute.For<IHostApplicationLifetime>();
    }

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
        _applicationLifetime.Received(1).StopApplication();
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
        _applicationLifetime.Received(1).StopApplication();
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
        _applicationLifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task ExecuteAsync_WhenWebSocketThrowsException_ShouldNotStopApplication()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var worker = GivenWorkerWithOptions(new WebSocketOptions
        {
            HealthCheckIntervalSeconds = 0,
            ErrorDelayIntervalSeconds = 0
        });

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
            });

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        listeningCalls.ShouldBe(2);
        _applicationLifetime.Received(1).StopApplication();
    }

    private WebSocketWorker GivenWorkerWithOptions(WebSocketOptions options)
    {
        return new WebSocketWorker(
            Substitute.For<ILogger<WebSocketWorker>>(),
            _webSocketClient,
            _applicationLifetime,
            Options.Create(options)
        );
    }
}