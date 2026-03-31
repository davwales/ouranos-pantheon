using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

public class SymbolGroupMember
{
    protected SymbolGroupMember() { }

    public Id<SymbolGroup> SymbolGroupId { get; private set; }

    public Id<Symbol> SymbolId { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    private Symbol? _symbol;
    public Symbol Symbol => _symbol ?? throw new NavigationPropertyNotLoadedException<SymbolGroupMember>();

    public static SymbolGroupMember Create(
        Id<SymbolGroup> symbolGroupId,
        Id<Symbol> symbolId,
        Symbol? symbol = null
    )
    {
        if (symbol is not null)
        {
            Guard.Against.InvalidInput(symbol, nameof(symbol), s => s.Id == symbolId);
        }

        return new SymbolGroupMember
        {
            SymbolGroupId = symbolGroupId,
            SymbolId = symbolId,
            AddedAt = DateTimeOffset.UtcNow,
            _symbol = symbol
        };
    }
}
