using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;

public interface IListener<in T>
{
    Task HandleMessageAsync(
        T message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    );
}
