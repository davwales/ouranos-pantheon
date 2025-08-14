using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Trades;

public class Trade : BaseEntity<Id<Trade>>
{
    private Trade()
    {
    }

    public Trade(
        Id<Trade> id,
        Id<Symbol> symbolId,
        decimal price,
        decimal volume,
        DateTimeOffset timestamp
    ) : base(id)
    {
        Guard.Against.Null(symbolId);

        Price = price;
        Volume = volume;
        SymbolId = symbolId;
        CreatedAt = timestamp;
    }

    public Id<Symbol> SymbolId { get; init; }

    public decimal Price { get; init; }

    public decimal Volume { get; init; }

    public virtual required Symbol Symbol { get; init; }
}