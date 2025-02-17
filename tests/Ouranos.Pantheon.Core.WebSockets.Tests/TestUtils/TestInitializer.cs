using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.TestUtils;

public sealed class TestInitializer : IWebSocketInitializer
{
    public async Task OnConnectedAsync(IWebSocketClient client, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}