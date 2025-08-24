using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Trades;

public class Trade : BaseEntity<Id<Trade>>
{
    protected Trade(Id<Trade> id) : base(id)
    {
    }

    public Id<Symbol> SymbolId { get; init; }

    public decimal Price { get; init; }

    public decimal Volume { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public virtual required Symbol Symbol { get; init; }

    public static Trade Create(
        Id<Trade> id,
        Symbol symbol,
        decimal price,
        decimal volume,
        DateTimeOffset timestamp
    )
    {
        Guard.Against.Null(symbol);

        return new Trade(id)
        {
            Price = price,
            Volume = volume,
            SymbolId = symbol.Id,
            Timestamp = timestamp,
            Symbol = symbol
        };
    }
}