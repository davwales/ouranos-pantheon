using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Listeners;

public sealed class ListenerDispatcherTests
{
    private readonly ListenerDispatcher<TestEntity> _dispatcher;
    private readonly IListener<TestEntity> _listener;

    public ListenerDispatcherTests()
    {
        _listener = Substitute.For<IListener<TestEntity>>();
        _dispatcher = new ListenerDispatcher<TestEntity>(_listener);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenGivenCorrectType_ShouldInvokeListener()
    {
        // Arrange
        var fixture = new Fixture();
        var message = fixture.Create<TestEntity>();
        var webSocketClient = Substitute.For<IWebSocketClient>();
        var cts = new CancellationTokenSource();

        // Act
        await _dispatcher.HandleMessageAsync(message, webSocketClient, cts.Token);

        // Assert
        await _listener.Received(1).HandleMessageAsync(message, webSocketClient, cts.Token);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenGivenInvalidType_ShouldNotInvokeListener()
    {
        // Arrange
        var webSocketClient = Substitute.For<IWebSocketClient>();
        var cts = new CancellationTokenSource();

        // Act
        await _dispatcher.HandleMessageAsync(1, webSocketClient, cts.Token);

        // Assert
        await _listener
            .Received(0)
            .HandleMessageAsync(
                Arg.Any<TestEntity>(),
                Arg.Any<IWebSocketClient>(),
                Arg.Any<CancellationToken>()
            );
    }
}
