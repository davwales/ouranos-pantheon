using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Subscriptions;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Subscriptions;

public sealed class SetupSubscriptions : ISetupSubscriptions
{
    private readonly IWebSocketClient _client;
    private readonly ILogger<SetupSubscriptions> _logger;

    private readonly IBsonSerializer<ClientMessage> _serializer =
        BsonSerializer.SerializerRegistry.GetSerializer<ClientMessage>();

    private readonly IReadOnlyCollection<int> _worlds;

    public SetupSubscriptions(
        ILogger<SetupSubscriptions> logger,
        IWebSocketClient client,
        IOptions<UniversalisOptions> options
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options.Value);

        _logger = logger;
        _client = client;
        _worlds = options.Value.Worlds;
    }

    public async Task Setup(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to setup Universalis WebSocket subscriptions.");
        cancellationToken.ThrowIfCancellationRequested();

        if (_worlds.Count == 0)
        {
            _logger.LogTrace("No worlds configured, subscribing to all trades.");
            await AddGlobalSubscriptions(cancellationToken);
        }
        else
        {
            _logger.LogTrace("'{worldCount}' configured, subscribing to trades on those worlds.", _worlds.Count);
            await AddWorldSpecificSubscriptions(cancellationToken);
        }

        _logger.LogDebug("Successfully setup Universalis WebSocket subscriptions.");
    }

    private async Task AddGlobalSubscriptions(CancellationToken cancellationToken = default)
    {
        var clientMessage = new ClientMessage("subscribe", "sales/add");
        var message = clientMessage.ToBson(writerSettings: new BsonBinaryWriterSettings());
        await _client.SendAsync(message, cancellationToken);
    }

    private async Task AddWorldSpecificSubscriptions(CancellationToken cancellationToken = default)
    {
        foreach (var world in _worlds)
        {
            var clientMessage = new ClientMessage("subscribe", $"sales/add{{world={world}}}");
            var message = clientMessage.ToBson();
            await _client.SendAsync(message, cancellationToken);
        }
    }
}