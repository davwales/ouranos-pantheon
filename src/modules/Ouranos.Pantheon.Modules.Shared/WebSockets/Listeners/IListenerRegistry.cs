using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

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
