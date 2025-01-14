using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Listeners;

public interface IListenerDispatcher
{
    Task HandleMessageAsync(
        object message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}