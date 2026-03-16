namespace Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

public interface IWebSocketInitializer
{
    Task OnConnectedAsync(IWebSocketClient client, CancellationToken cancellationToken = default);
}