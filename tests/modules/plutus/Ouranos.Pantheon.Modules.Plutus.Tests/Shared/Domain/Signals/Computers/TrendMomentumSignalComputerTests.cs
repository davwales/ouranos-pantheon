using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Signals.Computers;

public sealed class TrendMomentumSignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static TrendMomentumSignalComputer BuildComputer(decimal threshold = 0.05m)
    {
        var options = Options.Create(new SignalOptions { MomentumThreshold = threshold });
        return new TrendMomentumSignalComputer(options);
    }

    private static MarketTradeSnapshot MakeSnapshot(
        TimeFrame frame,
        decimal avgPrice,
        decimal volume = 1000m
    )
    {
        return new MarketTradeSnapshot(
            MarketId,
            SymbolId,
            frame,
            avgPrice * volume,
            avgPrice * 0.9m,
            avgPrice * 1.1m,
            volume,
            100,
            1000m,
            0m
        );
    }

    [Fact]
    public void Compute_WhenShortAverageDeltaEqualsThreshold_ReturnsOne()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 105m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = BuildComputer(threshold: 0.05m).Compute(context);

        // Assert
        result.ShouldBe(1m);
    }

    [Fact]
    public void Compute_WhenShortAverageIsLessThanLong_ReturnsBearish()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 95m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = BuildComputer(threshold: 0.05m).Compute(context);

        // Assert
        result.ShouldBe(-1m);
    }

    [Fact]
    public void Compute_WhenShortAndLongAveragesAreEqual_ReturnsZero()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 100m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = BuildComputer().Compute(context);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void Compute_WhenSnapshotsAreMissing_ReturnsNull()
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
        var result = BuildComputer().Compute(context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Compute_WhenMomentumThresholdIsZero_ReturnsNull()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 105m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = BuildComputer(threshold: 0m).Compute(context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Compute_WhenLongSnapshotHasZeroVolume_ReturnsNull()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 105m, volume: 1000m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m, volume: 0m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = BuildComputer().Compute(context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Metadata_ShouldExposeExpectedValues()
    {
        // Arrange & Act
        var computer = BuildComputer();

        // Assert
        computer.Type.ShouldBe(SignalType.TrendMomentum);
        computer.Label.ShouldBe("Trend Momentum");
        computer.Description.ShouldNotBeNullOrEmpty();
        computer.Intents.ShouldNotBeEmpty();
    }
}
