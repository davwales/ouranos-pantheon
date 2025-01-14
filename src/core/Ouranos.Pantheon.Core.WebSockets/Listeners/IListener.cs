using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Listeners;

public interface IListener<in T>
{
    Task HandleMessageAsync(
        T message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}