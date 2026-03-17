namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

public interface IQueueTradeMessages
{
    Task QueueMessages(
        IReadOnlyCollection<TradeMessage> messages,
        CancellationToken cancellationToken = default
    );
}
