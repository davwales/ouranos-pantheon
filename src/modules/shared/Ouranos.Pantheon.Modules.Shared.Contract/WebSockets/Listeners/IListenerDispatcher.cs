using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;

public interface IListenerDispatcher
{
    Task HandleMessageAsync(
        object message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}
