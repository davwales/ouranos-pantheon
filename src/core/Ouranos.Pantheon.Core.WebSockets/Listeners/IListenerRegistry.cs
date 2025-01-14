using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Listeners;

public interface IListenerRegistry
{
    void RegisterListener<T>(IListener<T> listener);

    Task HandleMessageAsync(
        byte[] messageData,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}