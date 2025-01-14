namespace Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

public interface IWebSocketInitializer
{
    Task OnConnectedAsync(IWebSocketClient client, CancellationToken cancellationToken = default);
}