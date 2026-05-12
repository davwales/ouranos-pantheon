using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

public class Trade : BaseEntity<Id<Trade>>
{
    protected Trade(Id<Trade> id)
        : base(id) { }

    public Id<Symbol> SymbolId { get; init; }

    public decimal Price { get; init; }

    public decimal Volume { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    private Symbol? _symbol;
    public Symbol Symbol => _symbol ?? throw new NavigationPropertyNotLoadedException<Trade>();

    public static Trade Create(
        Id<Trade> id,
        Id<Symbol> symbolId,
        decimal price,
        decimal volume,
        DateTimeOffset timestamp,
        Symbol? symbol = null
    )
    {
        if (symbol is not null)
        {
            Guard.Against.InvalidInput(symbol, nameof(symbol), s => s.Id == symbolId);
        }

        return new Trade(id)
        {
            Price = price,
            Volume = volume,
            SymbolId = symbolId,
            Timestamp = timestamp,
            _symbol = symbol,
        };
    }
}
