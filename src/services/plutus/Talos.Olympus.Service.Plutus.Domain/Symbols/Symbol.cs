using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;

namespace Talos.Olympus.Service.Plutus.Domain.Symbols;

public sealed class Symbol : BaseEntity<Id<Symbol>>
{
    public Symbol(
        Id<Symbol> id,
        string code,
        string? subcode,
        string name,
        Id<Market> marketId,
        Dictionary<string, object> additionalFields
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

    public string Name { get; init; }

    public Id<Market> MarketId { get; init; }

    public Dictionary<string, object> AdditionalFields { get; init; }
}