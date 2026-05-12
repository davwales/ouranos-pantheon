using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

public class Signal : BaseEntity<Id<Signal>>
{
    private Signal(Id<Signal> id)
        : base(id) { }

    public Id<Market> MarketId { get; private set; }

    public Id<Symbol> SymbolId { get; private set; }

    public SignalType Type { get; private set; }

    public decimal Value { get; private set; }

    public DateTimeOffset ComputedAt { get; private set; }

    private Market? _market;

    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<Signal>();

    private Symbol? _symbol;

    public Symbol Symbol => _symbol ?? throw new NavigationPropertyNotLoadedException<Signal>();

    public static Signal Create(
        Id<Market> marketId,
        Id<Symbol> symbolId,
        SignalType type,
        decimal value,
        Market? market = null,
        Symbol? symbol = null
    )
    {
        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        if (symbol is not null)
        {
            Guard.Against.InvalidInput(symbol, nameof(symbol), s => s.Id == symbolId);
        }

        return new Signal(DatabaseExtensions.CreateId<Signal>())
        {
            MarketId = marketId,
            SymbolId = symbolId,
            Type = type,
            Value = value,
            ComputedAt = DateTimeOffset.UtcNow,
            _market = market,
            _symbol = symbol,
        };
    }
}
