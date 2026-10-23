using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Signals.Computers;

public sealed class TaxAdjustedRoiSignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static TaxAdjustedRoiSignalComputer BuildComputer(decimal roiThreshold = 0.1m)
    {
        var options = Options.Create(new SignalOptions { RoiThreshold = roiThreshold });
        return new TaxAdjustedRoiSignalComputer(options);
    }

    private static SignalComputeContext BuildContext(
        decimal minPrice,
        decimal maxPrice,
        FlatTax? tax = null
    )
    {
        var rules = new MarketRules(tax);
        var snapshot = new MarketTradeSnapshot(
            MarketId,
            SymbolId,
            TimeFrame.OneWeek,
            maxPrice * 1000m,
            minPrice,
            maxPrice,
            1000m,
            100,
            1000m,
            rules.UnitTax(maxPrice)
        );

        return new SignalComputeContext(SymbolId, MarketId, rules, 1000m, null, snapshot, null, []);
    }

    [Fact]
    public async Task ComputeAsync_WhenRoiEqualsThreshold_ReturnsOne()
    {
        // Arrange
        var context = BuildContext(minPrice: 1000m, maxPrice: 1100m);

        // Act
        var result = await BuildComputer(roiThreshold: 0.1m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenTaxEliminatesSpread_ReturnsBearish()
    {
        // Arrange
        var context = BuildContext(
            minPrice: 1000m,
            maxPrice: 1000m,
            tax: new FlatTax(Minimum: 1m, Maximum: 5_000_000m, Rate: 0.1m)
        );

        // Act
        var result = await BuildComputer(roiThreshold: 0.1m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(-1m);
    }

    [Theory]
    [InlineData(1000, 1100)]
    [InlineData(1000, 1025.5)]
    [InlineData(40, 49)]
    [InlineData(290000000, 300000000)]
    public async Task ComputeAsync_WhenMarketIsTaxed_ShouldApplyTheMarketTaxRule(
        decimal minPrice,
        decimal maxPrice
    )
    {
        // Arrange
        var tax = new FlatTax(Minimum: 1m, Maximum: 5_000_000m, Rate: 0.02m);
        var context = BuildContext(minPrice, maxPrice, tax);
        var expectedTax = new MarketRules(tax).UnitTax(maxPrice);

        // Act
        var result = await BuildComputer(roiThreshold: 10m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe((maxPrice - minPrice - expectedTax) / minPrice / 10m);
    }

    [Fact]
    public async Task ComputeAsync_WhenTaxIsCapped_ShouldChargeTheMaximumNotTheFlatRate()
    {
        // Arrange
        var tax = new FlatTax(Minimum: 1m, Maximum: 5_000_000m, Rate: 0.02m);
        var context = BuildContext(minPrice: 290_000_000m, maxPrice: 300_000_000m, tax);

        // Act
        var result = await BuildComputer(roiThreshold: 1m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(5_000_000m / 290_000_000m);
    }

    [Fact]
    public async Task ComputeAsync_WhenMediumSnapshotIsMissing_ReturnsNull()
    {
        // Arrange
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            null,
            null,
            null,
            []
        );

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenMinPriceIsZero_ReturnsNull()
    {
        // Arrange
        var context = BuildContext(minPrice: 0m, maxPrice: 1000m);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenRoiThresholdIsZero_ReturnsNull()
    {
        // Arrange
        var context = BuildContext(minPrice: 1000m, maxPrice: 1100m);

        // Act
        var result = await BuildComputer(roiThreshold: 0m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Metadata_ShouldExposeExpectedValues()
    {
        // Arrange & Act
        var computer = BuildComputer();

        // Assert
        computer.Type.ShouldBe(SignalType.TaxAdjustedRoi);
        computer.Label.ShouldBe("Tax-Adjusted ROI");
        computer.Description.ShouldNotBeNullOrEmpty();
        computer.Intents.ShouldNotBeEmpty();
    }
}
