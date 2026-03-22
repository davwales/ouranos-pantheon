using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

public class Signal : BaseEntity<Id<Signal>>
{
    private Signal(Id<Signal> id) : base(id)
    {
    }

    public Id<Market> MarketId { get; private set; }

    public Id<Symbol> SymbolId { get; private set; }

    public SignalType Type { get; private set; }

    public decimal Value { get; private set; }

    public DateTimeOffset ComputedAt { get; private set; }

    public static Signal Create(
        Id<Market> marketId,
        Id<Symbol> symbolId,
        SignalType type,
        decimal value
    ) => new(DatabaseExtensions.CreateId<Signal>())
    {
        MarketId = marketId,
        SymbolId = symbolId,
        Type = type,
        Value = value,
        ComputedAt = DateTimeOffset.UtcNow,
    };
}
