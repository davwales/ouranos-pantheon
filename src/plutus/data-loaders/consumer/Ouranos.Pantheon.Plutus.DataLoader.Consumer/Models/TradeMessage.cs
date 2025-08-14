using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Models;

public sealed class TradeMessage : BaseEntity<Id<TradeMessage>>
{
    public TradeMessage(
        Id<TradeMessage> id,
        Id<Trade> tradeId,
        Guid messageId
    ) : base(id)
    {
        ArgumentNullException.ThrowIfNull(TradeId);

        TradeId = tradeId;
        MessageId = messageId;
    }

    public Id<Trade> TradeId { get; init; }

    public Guid MessageId { get; init; }
}