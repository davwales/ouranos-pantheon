using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;

public sealed class TestInitializer : IWebSocketInitializer
{
    public async Task OnConnectedAsync(
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        await Task.CompletedTask;
    }
}
