using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;

public sealed class SymbolGroup : BaseEntity<Id<SymbolGroup>>
{
    private SymbolGroup()
    {
    }

    public SymbolGroup(
        Id<SymbolGroup> id,
        Id<Market> marketId,
        string name,
        List<Id<Symbol>> symbolIds
    ) : base(id)
    {
        Guard.Against.Null(marketId);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(symbolIds);

        MarketId = marketId;
        Name = name;
        SymbolIds = symbolIds;
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; init; }

    public List<Id<Symbol>> SymbolIds { get; init; }
}