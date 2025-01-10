using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Trades;

public sealed class Trade : BaseEntity<Id<Trade>>
{
    public Trade(
        Id<Trade> id,
        decimal price,
        decimal volume,
        TradeMetadata metadata,
        DateTimeOffset timestamp
    ) : base(id)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        Price = price;
        Volume = volume;
        Metadata = metadata;
        CreatedAt = timestamp;
    }

    public decimal Price { get; init; }

    public decimal Volume { get; init; }

    public TradeMetadata Metadata { get; init; }
}