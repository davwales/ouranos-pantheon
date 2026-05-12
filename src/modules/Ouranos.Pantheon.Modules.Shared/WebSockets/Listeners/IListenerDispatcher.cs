using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

public interface IListenerDispatcher
{
    Task HandleMessageAsync(
        object message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}
