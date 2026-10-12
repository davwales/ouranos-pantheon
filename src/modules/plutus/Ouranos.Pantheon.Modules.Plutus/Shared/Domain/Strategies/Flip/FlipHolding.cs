using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

internal sealed class FlipHolding(
    Id<Symbol> symbolId,
    string symbolName,
    DateTimeOffset openedAt,
    int openedBar,
    decimal lastLowPrice
) : Holding(symbolId, symbolName, openedAt)
{
    public int OpenedBar { get; } = openedBar;

    public decimal Listed { get; set; }

    public decimal Ask { get; set; }

    public decimal LastLowPrice { get; set; } = lastLowPrice;
}
