using Ardalis.GuardClauses;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

public sealed class QueueTradeMessages : IQueueTradeMessages
{
    private readonly IPlutusDataBus _bus;
    private readonly ILogger<QueueTradeMessages> _logger;

    public QueueTradeMessages(ILogger<QueueTradeMessages> logger, IPlutusDataBus bus)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(bus);

        _logger = logger;
        _bus = bus;
    }

    public async Task QueueMessages(
        IReadOnlyCollection<TradeMessage> messages,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to queue '{messageCount}' trade messages.", messages.Count);
        cancellationToken.ThrowIfCancellationRequested();

        if (messages.Count == 0)
        {
            _logger.LogDebug("No trade messages to queue.");
            return;
        }

        await _bus.PublishBatch(messages, cancellationToken);

        _logger.LogDebug("Successfully queued trade message.");
    }
}
