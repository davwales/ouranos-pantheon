using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

public sealed class Symbol : BaseEntity<Id<Symbol>>
{
    public Symbol(
        Id<Symbol> id,
        string code,
        string? subCode,
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
        SubCode = subCode;
        Name = name;
        MarketId = marketId;
        AdditionalFields = additionalFields;
    }

    public string Code { get; init; }

    public string? SubCode { get; init; }

    public string Name { get; init; }

    public Id<Market> MarketId { get; init; }

    public AdditionalFields AdditionalFields { get; init; }
}