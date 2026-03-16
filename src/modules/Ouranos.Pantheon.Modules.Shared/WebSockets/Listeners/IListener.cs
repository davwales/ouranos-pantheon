using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

public interface IListener<in T>
{
    Task HandleMessageAsync(
        T message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}