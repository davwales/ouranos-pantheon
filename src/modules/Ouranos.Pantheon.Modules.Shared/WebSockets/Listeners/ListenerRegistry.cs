using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

public sealed class ListenerRegistry : IListenerRegistry
{
    private readonly Dictionary<Type, List<IListenerDispatcher>> _listeners = new();
    private readonly IMessageSerializer _serializer;

    public ListenerRegistry(IMessageSerializer serializer)
    {
        Guard.Against.Null(serializer);
        _serializer = serializer;
    }

    public IReadOnlyDictionary<Type, IReadOnlyList<IListenerDispatcher>> Listeners =>
        _listeners.ToDictionary(x => x.Key, IReadOnlyList<IListenerDispatcher> (x) => x.Value);

    public void RegisterListener<T>(IListener<T> listener)
    {
        var messageType = typeof(T);

        if (!_listeners.TryGetValue(messageType, out var value))
        {
            value = [];
            _listeners[messageType] = value;
        }

        var dispatcher = new ListenerDispatcher<T>(listener);
        value.Add(dispatcher);
    }

    public async Task HandleMessageAsync(
        byte[] messageData,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        var message = _serializer.Deserialize<object>(messageData);

        if (message is IEnumerable<object> messages)
        {
            var tasks = messages.SelectMany(m => GetTasksForMessage(m, client, cancellationToken));
            await Task.WhenAll(tasks);
        }
        else
        {
            var tasks = GetTasksForMessage(message, client, cancellationToken);
            await Task.WhenAll(tasks);
        }
    }

    private IEnumerable<Task> GetTasksForMessage(
        object message,
        IWebSocketClient client,
        CancellationToken cancellationToken
    )
    {
        return _listeners
            .Where(kvp => kvp.Key.IsInstanceOfType(message))
            .SelectMany(kvp => kvp.Value)
            .Select(listener => listener.HandleMessageAsync(message, client, cancellationToken));
    }
}
