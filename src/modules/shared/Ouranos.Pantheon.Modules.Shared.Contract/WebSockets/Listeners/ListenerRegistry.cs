using System.Diagnostics;
using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;

public sealed class ListenerRegistry(IMessageSerializer serializer, WebSocketTelemetry telemetry)
    : IListenerRegistry
{
    private readonly Dictionary<Type, List<IListenerDispatcher>> _listeners = [];
    private readonly IMessageSerializer _serializer = Guard.Against.Null(serializer);
    private readonly WebSocketTelemetry _telemetry = Guard.Against.Null(telemetry);

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
        using var span = _telemetry.MessageDispatch(client.Host, messageData.Length);
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            var message = _serializer.Deserialize<object>(messageData);

            span.SetMessageType(ResolveMessageTypeName(message));

            if (message is IEnumerable<object> messages)
            {
                var tasks = messages.SelectMany(m =>
                    GetTasksForMessage(m, client, cancellationToken)
                );
                await Task.WhenAll(tasks);
            }
            else
            {
                var tasks = GetTasksForMessage(message, client, cancellationToken);
                await Task.WhenAll(tasks);
            }
        }
        catch (Exception ex)
        {
            span.Fail(ex);
            throw;
        }
        finally
        {
            _telemetry.RecordMessageReceived(
                client.Host,
                messageData.Length,
                Stopwatch.GetElapsedTime(startTimestamp)
            );
        }
    }

    private static string ResolveMessageTypeName(object message)
    {
        if (message is IEnumerable<object> messages)
        {
            var first = messages.FirstOrDefault();
            return first?.GetType().Name ?? message.GetType().Name;
        }

        return message.GetType().Name;
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
