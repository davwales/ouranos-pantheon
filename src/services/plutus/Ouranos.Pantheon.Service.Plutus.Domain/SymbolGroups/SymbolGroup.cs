using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;

public sealed class SymbolGroup : BaseEntity<Id<SymbolGroup>>
{
    public SymbolGroup(
        Id<SymbolGroup> id,
        Id<Market> marketId,
        string name,
        IReadOnlyList<Id<Symbol>> symbolIds
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

    public IReadOnlyList<Id<Symbol>> SymbolIds { get; init; }
}