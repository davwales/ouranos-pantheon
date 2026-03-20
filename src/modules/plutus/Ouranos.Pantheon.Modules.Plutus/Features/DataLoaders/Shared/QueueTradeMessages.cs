using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Wolverine.Runtime;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

public sealed class QueueTradeMessages : IQueueTradeMessages
{
    private readonly ILogger<QueueTradeMessages> _logger;
    private readonly IWolverineRuntime _wolverineRuntime;

    public QueueTradeMessages(ILogger<QueueTradeMessages> logger, IWolverineRuntime wolverineRuntime)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(wolverineRuntime);
        _logger = logger;
        _wolverineRuntime = wolverineRuntime;
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

        var bus = new MessageBus(_wolverineRuntime);

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await bus.PublishAsync(message);
        }

        _logger.LogDebug("Successfully queued '{messageCount}' trade messages.", messages.Count);
    }
}
