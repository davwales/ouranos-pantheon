using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Messages;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Initializers;

public sealed class SubscriptionInitializer : IWebSocketInitializer
{
    private readonly ILogger<SubscriptionInitializer> _logger;
    private readonly IReadOnlyCollection<int> _worlds;

    public SubscriptionInitializer(
        ILogger<SubscriptionInitializer> logger,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _worlds = configuration.GetSection("Ouranos:Universalis:Worlds").Get<List<int>>() ?? [];
    }

    public async Task OnConnectedAsync(
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to setup WebSocket subscriptions.");
        cancellationToken.ThrowIfCancellationRequested();

        if (_worlds.Count == 0)
        {
            _logger.LogTrace("No worlds configured, subscribing to all trades.");
            await AddGlobalSubscriptions(client, cancellationToken);
        }
        else
        {
            _logger.LogTrace("'{worldCount}' configured, subscribing to trades on those worlds.", _worlds.Count);
            await AddWorldSpecificSubscriptions(client, cancellationToken);
        }

        _logger.LogDebug("Successfully setup WebSocket subscriptions.");
    }

    private static async Task AddGlobalSubscriptions(IWebSocketClient client,
        CancellationToken cancellationToken = default)
    {
        var message = new SubscribeMessage("sales/add");
        await client.SendAsync(message, cancellationToken);
    }

    private async Task AddWorldSpecificSubscriptions(IWebSocketClient client,
        CancellationToken cancellationToken = default)
    {
        foreach (var world in _worlds)
        {
            var message = new SubscribeMessage($"sales/add{{world={world}}}");
            await client.SendAsync(message, cancellationToken);
        }
    }
}