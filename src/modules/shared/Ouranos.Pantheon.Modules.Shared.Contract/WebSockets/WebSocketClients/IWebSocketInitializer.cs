namespace Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

public interface IWebSocketInitializer
{
    Task OnConnectedAsync(IWebSocketClient client, CancellationToken cancellationToken = default);
}
