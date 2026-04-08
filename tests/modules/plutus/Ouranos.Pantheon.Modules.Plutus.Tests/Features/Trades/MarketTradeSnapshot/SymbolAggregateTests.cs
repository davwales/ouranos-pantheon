using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.MarketSnapshot;

public sealed class SymbolAggregateTests
{
    [Fact]
    public void SymbolAggregate_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var symbol = Symbol.Create(
            symbolId,
            "Gold",
            "AU",
            "Gold Ore",
            new Id<Market>(Guid.NewGuid().ToString()),
            new AdditionalFields()
        );

        // Act
        var aggregate = new SymbolAggregate(
            Symbol: symbol,
            TotalSpent: 10000m,
            MinPrice: 90m,
            MaxPrice: 110m,
            TotalVolume: 100m,
            NumTx: 50,
            Limit: 200m
        );

        // Assert
        aggregate.Symbol.ShouldBe(symbol);
        aggregate.TotalSpent.ShouldBe(10000m);
        aggregate.MinPrice.ShouldBe(90m);
        aggregate.MaxPrice.ShouldBe(110m);
        aggregate.TotalVolume.ShouldBe(100m);
        aggregate.NumTx.ShouldBe(50);
        aggregate.Limit.ShouldBe(200m);
    }
}
