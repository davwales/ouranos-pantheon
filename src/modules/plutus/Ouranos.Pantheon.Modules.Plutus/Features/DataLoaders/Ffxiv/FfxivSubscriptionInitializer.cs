using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;

public sealed class FfxivSubscriptionInitializer : IWebSocketInitializer
{
    private readonly ILogger<FfxivSubscriptionInitializer> _logger;
    private readonly IReadOnlyCollection<int> _worlds;

    public FfxivSubscriptionInitializer(
        ILogger<FfxivSubscriptionInitializer> logger,
        IOptions<FfxivDataLoaderOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(options);

        _logger = logger;
        _worlds = options.Value.Worlds;
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
            _logger.LogTrace(
                "'{worldCount}' configured, subscribing to trades on those worlds.",
                _worlds.Count
            );
            await AddWorldSpecificSubscriptions(client, cancellationToken);
        }

        _logger.LogDebug("Successfully setup WebSocket subscriptions.");
    }

    private static async Task AddGlobalSubscriptions(
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        var message = new SubscribeMessage("sales/add");
        await client.SendAsync(message, cancellationToken);
    }

    private async Task AddWorldSpecificSubscriptions(
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var world in _worlds)
        {
            var message = new SubscribeMessage($"sales/add{{world={world}}}");
            await client.SendAsync(message, cancellationToken);
        }
    }
}
