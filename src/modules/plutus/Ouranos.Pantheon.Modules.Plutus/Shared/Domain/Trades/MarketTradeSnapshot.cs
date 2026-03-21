using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

public class MarketTradeSnapshot : BaseEntity<Id<MarketTradeSnapshot>>
{
    private MarketTradeSnapshot(Id<MarketTradeSnapshot> id) : base(id) { }

    public Id<Market> MarketId { get; private set; }

    public Id<Symbol> SymbolId { get; private set; }

    public TimeFrame TimeFrame { get; private set; }

    public decimal TotalSpent { get; private set; }

    public decimal MinPrice { get; private set; }

    public decimal MaxPrice { get; private set; }

    public decimal TotalVolume { get; private set; }

    public int NumTransactions { get; private set; }

    public decimal Limit { get; private set; }

    public decimal Tax { get; private set; }

    public virtual Market? Market { get; init; }

    public virtual Symbol? Symbol { get; init; }

    public static MarketTradeSnapshot Create(
        Id<Market> marketId,
        Id<Symbol> symbolId,
        TimeFrame timeFrame,
        decimal totalSpent,
        decimal minPrice,
        decimal maxPrice,
        decimal totalVolume,
        int numTransactions,
        decimal limit,
        decimal tax
    ) => new(DatabaseExtensions.CreateId<MarketTradeSnapshot>())
    {
        MarketId = marketId,
        SymbolId = symbolId,
        TimeFrame = timeFrame,
        TotalSpent = totalSpent,
        MinPrice = minPrice,
        MaxPrice = maxPrice,
        TotalVolume = totalVolume,
        NumTransactions = numTransactions,
        Limit = limit,
        Tax = tax,
    };
}
