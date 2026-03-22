using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

public class Symbol : BaseEntity<Id<Symbol>>
{
    protected Symbol(Id<Symbol> id) : base(id)
    {
        Code = string.Empty;
        Name = string.Empty;
        AdditionalFields = new AdditionalFields();
    }

    public string Code { get; init; }

    public string? Subcode { get; init; }

    public string Name { get; private set; }

    public Id<Market> MarketId { get; init; }

    public AdditionalFields AdditionalFields { get; private set; }

    private Market? _market;
    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<Symbol>();

    public static Symbol Create(
        Id<Symbol> id,
        string code,
        string? subcode,
        string name,
        Id<Market> marketId,
        AdditionalFields additionalFields,
        Market? market = null
    )
    {
        Guard.Against.NullOrWhiteSpace(code);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(additionalFields);

        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        return new Symbol(id)
        {
            Code = code,
            Subcode = subcode,
            Name = name,
            MarketId = marketId,
            AdditionalFields = additionalFields,
            _market = market
        };
    }

    public void Update(string name, AdditionalFields additionalFields)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(additionalFields);

        Name = name;
        AdditionalFields = additionalFields;
    }
}
