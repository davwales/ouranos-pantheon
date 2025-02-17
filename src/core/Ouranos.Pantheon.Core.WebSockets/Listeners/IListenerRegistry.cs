using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Listeners;

public interface IListenerRegistry
{
    IReadOnlyDictionary<Type, IReadOnlyList<IListenerDispatcher>> Listeners { get; }

    void RegisterListener<T>(IListener<T> listener);

    Task HandleMessageAsync(
        byte[] messageData,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}