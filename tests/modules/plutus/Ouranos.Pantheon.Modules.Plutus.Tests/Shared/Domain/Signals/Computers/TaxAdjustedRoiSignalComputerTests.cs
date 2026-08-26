using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
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
        decimal taxRate = 0m
    )
    {
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
            maxPrice * taxRate
        );

        return new SignalComputeContext(
            SymbolId,
            MarketId,
            taxRate,
            1000m,
            null,
            snapshot,
            null,
            []
        );
    }

    [Fact]
    public async Task ComputeAsync_WhenRoiEqualsThreshold_ReturnsOne()
    {
        // Arrange
        var context = BuildContext(minPrice: 1000m, maxPrice: 1100m, taxRate: 0m);

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
        var context = BuildContext(minPrice: 1000m, maxPrice: 1000m, taxRate: 0.1m);

        // Act
        var result = await BuildComputer(roiThreshold: 0.1m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(-1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenMediumSnapshotIsMissing_ReturnsNull()
    {
        // Arrange
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, []);

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
