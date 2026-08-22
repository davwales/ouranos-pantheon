using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Listeners;

public sealed class ListenerRegistryTests
{
    private readonly ListenerRegistry _listenerRegistry;
    private readonly IMessageSerializer _serializer;

    public ListenerRegistryTests()
    {
        _serializer = Substitute.For<IMessageSerializer>();
        _listenerRegistry = new ListenerRegistry(_serializer);
    }

    [Fact]
    public void RegisterListener_WhenSingleListener_ShouldRegisterListener()
    {
        // Arrange
        var listener = Substitute.For<IListener<TestEntity>>();
        var expectedDispatcher = new ListenerDispatcher<TestEntity>(listener);

        // Act
        _listenerRegistry.RegisterListener(listener);

        // Assert
        _listenerRegistry.Listeners.Keys.ShouldContain(typeof(TestEntity));
        _listenerRegistry.Listeners[typeof(TestEntity)].Count.ShouldBe(1);
        _listenerRegistry.Listeners[typeof(TestEntity)][0].ShouldBe(expectedDispatcher);
    }

    [Fact]
    public void RegisterListener_WhenMultipleListeners_ShouldRegisterAllListeners()
    {
        // Arrange
        var firstListener = Substitute.For<IListener<TestEntity>>();
        var expectedFirstDispatcher = new ListenerDispatcher<TestEntity>(firstListener);

        var secondListener = Substitute.For<IListener<TestEntity>>();
        var expectedSecondDispatcher = new ListenerDispatcher<TestEntity>(secondListener);

        // Act
        _listenerRegistry.RegisterListener(firstListener);
        _listenerRegistry.RegisterListener(secondListener);

        // Assert
        _listenerRegistry.Listeners.Keys.ShouldContain(typeof(TestEntity));
        _listenerRegistry.Listeners[typeof(TestEntity)].Count.ShouldBe(2);
        _listenerRegistry.Listeners[typeof(TestEntity)][0].ShouldBe(expectedFirstDispatcher);
        _listenerRegistry.Listeners[typeof(TestEntity)][1].ShouldBe(expectedSecondDispatcher);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenSingleMessage_ShouldInvokeAllListeners()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.Create<byte[]>();
        var entity = fixture.Create<TestEntity>();
        var webSocketClient = Substitute.For<IWebSocketClient>();
        var cts = new CancellationTokenSource();

        var firstListener = Substitute.For<IListener<TestEntity>>();
        _listenerRegistry.RegisterListener(firstListener);

        var secondListener = Substitute.For<IListener<TestEntity>>();
        _listenerRegistry.RegisterListener(secondListener);

        _serializer.Deserialize<object>(bytes).Returns(entity);

        // Act
        await _listenerRegistry.HandleMessageAsync(bytes, webSocketClient, cts.Token);

        // Assert
        await firstListener.Received(1).HandleMessageAsync(entity, webSocketClient, cts.Token);
        await secondListener.Received(1).HandleMessageAsync(entity, webSocketClient, cts.Token);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenManyMessages_ShouldInvokeAllListenersForEachMessage()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.Create<byte[]>();
        var entities = fixture.CreateMany<TestEntity>().ToList();
        var webSocketClient = Substitute.For<IWebSocketClient>();
        var cts = new CancellationTokenSource();

        var firstListener = Substitute.For<IListener<TestEntity>>();
        _listenerRegistry.RegisterListener(firstListener);

        var secondListener = Substitute.For<IListener<TestEntity>>();
        _listenerRegistry.RegisterListener(secondListener);

        _serializer.Deserialize<object>(bytes).Returns(entities);

        // Act
        await _listenerRegistry.HandleMessageAsync(bytes, webSocketClient, cts.Token);

        // Assert
        foreach (var entity in entities)
        {
            await firstListener.Received(1).HandleMessageAsync(entity, webSocketClient, cts.Token);
            await secondListener.Received(1).HandleMessageAsync(entity, webSocketClient, cts.Token);
        }
    }
}
