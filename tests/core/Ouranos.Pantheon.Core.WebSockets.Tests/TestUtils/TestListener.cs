using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.TestUtils;

public sealed class TestListener : IListener<TestEntity>
{
    public async Task HandleMessageAsync(
        TestEntity message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        await Task.CompletedTask;
    }
}