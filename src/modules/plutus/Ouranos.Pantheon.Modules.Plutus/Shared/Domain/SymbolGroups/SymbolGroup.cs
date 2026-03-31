using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

public class SymbolGroup : BaseEntity<Id<SymbolGroup>>
{
    protected SymbolGroup(Id<SymbolGroup> id) : base(id)
    {
        Name = string.Empty;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public Id<Market> MarketId { get; init; }

    private Market? _market;
    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<SymbolGroup>();

    private ICollection<SymbolGroupMember>? _members;
    public ICollection<SymbolGroupMember> Members => _members ?? throw new NavigationPropertyNotLoadedException<SymbolGroup>();

    public static SymbolGroup Create(
        Id<SymbolGroup> id,
        string name,
        string? description,
        Id<Market> marketId,
        Market? market = null,
        ICollection<SymbolGroupMember>? members = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);

        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        return new SymbolGroup(id)
        {
            Name = name,
            Description = description,
            MarketId = marketId,
            _market = market,
            _members = members
        };
    }

    public void Update(string name, string? description)
    {
        Guard.Against.NullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}
