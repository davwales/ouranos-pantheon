using Ouranos.Pantheon.Plutus.DataLoader.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Application.Interfaces.Trades;

public interface IQueueTradeMessages
{
    Task QueueMessages(
        IReadOnlyCollection<TradeMessage> messages,
        CancellationToken cancellationToken = default
    );
}