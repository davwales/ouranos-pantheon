using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;

public class SymbolGroup : BaseEntity<Id<SymbolGroup>>
{
    protected SymbolGroup(Id<SymbolGroup> id) : base(id)
    {
        Name = string.Empty;
        SymbolIds = [];
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; init; }

    public List<Id<Symbol>> SymbolIds { get; init; }

    public virtual required Market Market { get; init; }

    public virtual required IEnumerable<Symbol> Symbols { get; init; }

    public static SymbolGroup Create(
        Id<SymbolGroup> id,
        Market market,
        string name,
        List<Symbol> symbols
    )
    {
        Guard.Against.Null(market);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(symbols);

        return new SymbolGroup(id)
        {
            MarketId = market.Id,
            Name = name,
            SymbolIds = [.. symbols.Select(s => s.Id)],
            Market = market,
            Symbols = symbols
        };
    }
}