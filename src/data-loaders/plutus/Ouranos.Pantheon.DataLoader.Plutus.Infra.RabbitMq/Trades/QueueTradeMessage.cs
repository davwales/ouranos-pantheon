using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq.Trades;

public sealed class QueueTradeMessage : IQueueTradeMessage
{
    private readonly IBus _bus;
    private readonly ILogger<QueueTradeMessage> _logger;

    public QueueTradeMessage(ILogger<QueueTradeMessage> logger, IBus bus)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(bus);

        _logger = logger;
        _bus = bus;
    }

    public async Task QueueMessage(
        TradeMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to queue trade message '{@message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        await _bus.Publish(message, cancellationToken);

        _logger.LogDebug("Successfully queued trade message.");
    }
}