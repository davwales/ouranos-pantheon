using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.TestUtils;

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
