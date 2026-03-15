using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

public class TradeMessage : BaseEntity<Id<TradeMessage>>
{
    protected TradeMessage(Id<TradeMessage> id) : base(id)
    {
        TradeId = new Id<Trade>(Guid.NewGuid().ToString());
        MessageId = Guid.NewGuid();
    }

    public Id<Trade> TradeId { get; init; }

    public Guid MessageId { get; init; }

    public static TradeMessage Create(
        Id<TradeMessage> id,
        Id<Trade> tradeId,
        Guid messageId
    )
    {
        Guard.Against.Null(id);
        Guard.Against.Null(tradeId);
        Guard.Against.Null(messageId);

        return new TradeMessage(id) { TradeId = tradeId, MessageId = messageId };
    }
}