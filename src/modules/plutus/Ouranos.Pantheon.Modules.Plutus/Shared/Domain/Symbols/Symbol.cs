using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
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

    public virtual required Market Market { get; init; }

    public static Symbol Create(
        Id<Symbol> id,
        string code,
        string? subcode,
        string name,
        Market market,
        AdditionalFields additionalFields
    )
    {
        Guard.Against.NullOrWhiteSpace(code);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(market);
        Guard.Against.Null(additionalFields);

        return new Symbol(id)
        {
            Code = code,
            Subcode = subcode,
            Name = name,
            MarketId = market.Id,
            AdditionalFields = additionalFields,
            Market = market
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
