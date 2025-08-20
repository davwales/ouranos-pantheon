using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Trades;

public class TradeMessage : BaseEntity<Id<TradeMessage>>
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