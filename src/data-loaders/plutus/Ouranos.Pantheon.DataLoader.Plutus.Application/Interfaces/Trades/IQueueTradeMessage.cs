using Ouranos.Pantheon.DataLoader.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;

public interface IQueueTradeMessage
{
    Task QueueMessage(
        TradeMessage message,
        CancellationToken cancellationToken = default
    );
}