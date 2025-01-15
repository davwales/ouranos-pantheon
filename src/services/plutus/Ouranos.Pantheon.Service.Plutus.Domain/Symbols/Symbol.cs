using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

public sealed class Symbol : BaseEntity<Id<Symbol>>
{
    public Symbol(
        Id<Symbol> id,
        string code,
        string? subcode,
        string name,
        Id<Market> marketId,
        AdditionalFields additionalFields
    ) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(marketId);
        ArgumentNullException.ThrowIfNull(additionalFields);

        Code = code;
        Subcode = subcode;
        Name = name;
        MarketId = marketId;
        AdditionalFields = additionalFields;
    }

    public string Code { get; init; }

    public string? Subcode { get; init; }

    public string Name { get; private set; }

    public Id<Market> MarketId { get; init; }

    public AdditionalFields AdditionalFields { get; private set; }

    public void Update(string name, AdditionalFields additionalFields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(additionalFields);

        Name = name;
        AdditionalFields = additionalFields;
        base.Update();
    }
}