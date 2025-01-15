using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;

public interface IQueueTradeMessages
{
    Task QueueMessages(
        IReadOnlyCollection<TradeMessage> messages,
        CancellationToken cancellationToken = default
    );
}