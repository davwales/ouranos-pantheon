using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Trading;

public sealed class MarketRulesTests
{
    [Theory]
    [InlineData(49, 0)]
    [InlineData(50, 1)]
    [InlineData(1_234, 24)]
    [InlineData(1_000_000_000, 5_000_000)]
    public void UnitTax_ShouldRoundDownWaiveBelowMinimumAndCap(decimal price, decimal expected)
    {
        // Act
        var tax = FlipTestData.OsrsRules.UnitTax(price);

        // Assert
        tax.ShouldBe(expected);
    }

    [Fact]
    public void UnitTax_WhenMarketHasNoTax_ShouldBeZero()
    {
        // Act
        var tax = MarketRules.TaxFree.UnitTax(1_000m);

        // Assert
        tax.ShouldBe(0m);
    }

    [Fact]
    public void NetMargin_ShouldDeductTaxFromTheAsk()
    {
        // Act
        var margin = FlipTestData.OsrsRules.NetMargin(100m, 110m);

        // Assert
        margin.ShouldBe(0.08m);
    }

    [Theory]
    [InlineData(40, 40)]
    [InlineData(100, 102)]
    [InlineData(1_000_000_000, 1_005_000_000)]
    public void BreakEvenAsk_ShouldBeTheLowestAskThatRecoversCostAfterTax(
        decimal unitCost,
        decimal expected
    )
    {
        // Act
        var ask = FlipTestData.OsrsRules.BreakEvenAsk(unitCost);

        // Assert
        ask.ShouldBe(expected);
        (ask - FlipTestData.OsrsRules.UnitTax(ask)).ShouldBeGreaterThanOrEqualTo(unitCost);
    }

    [Theory]
    [InlineData(1_234, 10, 12_100)]
    [InlineData(49, 10, 490)]
    [InlineData(100, 0, 0)]
    public void NetSale_ShouldDeductUnitTaxFromEveryUnit(
        decimal price,
        decimal quantity,
        decimal expected
    )
    {
        // Act
        var net = FlipTestData.OsrsRules.NetSale(price, quantity);

        // Assert
        net.ShouldBe(expected);
    }

    [Fact]
    public void NetMargin_WhenBidIsZero_ShouldBeZero()
    {
        // Act
        var margin = FlipTestData.OsrsRules.NetMargin(0m, 110m);

        // Assert
        margin.ShouldBe(0m);
    }

    [Fact]
    public void For_WhenMarketHasFlatTax_ShouldUseIt()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Market",
            new Taxes(FlipTestData.OsrsTax)
        );

        // Act
        var rules = MarketRules.For(market);

        // Assert
        rules.Tax.ShouldBe(FlipTestData.OsrsTax);
    }

    [Fact]
    public void For_WhenMarketHasNoTax_ShouldBeTaxFree()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Market",
            new Taxes(null)
        );

        // Act
        var rules = MarketRules.For(market);

        // Assert
        rules.ShouldBe(MarketRules.TaxFree);
    }

    [Fact]
    public void For_WhenMarketIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => MarketRules.For(null!));
    }
}
