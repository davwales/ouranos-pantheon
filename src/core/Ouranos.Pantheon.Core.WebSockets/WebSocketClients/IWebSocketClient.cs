using System.Net.WebSockets;

namespace Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

public interface IWebSocketClient
{
    WebSocketState State { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task SendAsync(string message, CancellationToken cancellationToken = default);

    Task SendAsync(byte[] message, CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}