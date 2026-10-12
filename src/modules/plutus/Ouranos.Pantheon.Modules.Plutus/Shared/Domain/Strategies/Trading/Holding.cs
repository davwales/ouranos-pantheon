using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

/// <summary>
///     Inventory of one symbol from its first buy until it is sold out. Buys and sells may be
///     partial; the whole round trip becomes a single <see cref="BacktestPosition" />.
/// </summary>
public class Holding(Id<Symbol> symbolId, string symbolName, DateTimeOffset openedAt)
{
    public Id<Symbol> SymbolId { get; } = symbolId;

    public string SymbolName { get; } = symbolName;

    public DateTimeOffset OpenedAt { get; } = openedAt;

    public decimal Quantity { get; private set; }

    /// <summary>Cost basis of the units still held.</summary>
    public decimal Cost { get; private set; }

    public decimal BoughtQuantity { get; private set; }

    public decimal BoughtCost { get; private set; }

    public decimal SoldGross { get; private set; }

    public decimal SoldNet { get; private set; }

    public decimal UnitCost => Quantity > 0 ? Cost / Quantity : 0m;

    public void Buy(decimal quantity, decimal price)
    {
        Quantity += quantity;
        Cost += quantity * price;
        BoughtQuantity += quantity;
        BoughtCost += quantity * price;
    }

    /// <summary>Returns the closed round trip once the last unit is sold.</summary>
    public BacktestPosition? Sell(
        decimal quantity,
        decimal price,
        decimal netProceeds,
        DateTimeOffset time
    )
    {
        var unitCost = UnitCost;
        Cost -= quantity * unitCost;
        Quantity -= quantity;
        SoldGross += quantity * price;
        SoldNet += netProceeds;

        if (Quantity > 0)
        {
            return null;
        }

        var profit = SoldNet - BoughtCost;

        return new BacktestPosition(
            SymbolId.Value,
            SymbolName,
            BoughtQuantity > 0 ? BoughtCost / BoughtQuantity : 0m,
            BoughtQuantity > 0 ? SoldGross / BoughtQuantity : 0m,
            BoughtQuantity,
            profit,
            BoughtCost > 0 ? profit / BoughtCost : 0m,
            OpenedAt,
            time
        );
    }
}
