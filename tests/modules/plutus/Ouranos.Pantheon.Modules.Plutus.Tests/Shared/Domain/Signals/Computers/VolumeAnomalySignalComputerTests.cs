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

public sealed class VolumeAnomalySignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static VolumeAnomalySignalComputer BuildComputer(decimal threshold = 3.0m)
    {
        var options = Options.Create(new SignalOptions { VolumeAnomalyThreshold = threshold });
        return new VolumeAnomalySignalComputer(options);
    }

    private static MarketTradeSnapshot SnapshotWithRate(TimeFrame frame, decimal ratePerMinute)
    {
        var windowMinutes = (decimal)(frame.ToTimeSpan()?.TotalMinutes ?? 0);
        var totalVolume = ratePerMinute * windowMinutes;

        return new MarketTradeSnapshot(
            MarketId,
            SymbolId,
            frame,
            totalVolume * 100m,
            90m,
            110m,
            totalVolume,
            100,
            1000m,
            0m
        );
    }

    [Fact]
    public void Compute_WhenShortRateEqualsThresholdMultipleOfLongRate_ReturnsOne()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 3m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 1m);
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
        var result = BuildComputer(threshold: 3.0m).Compute(context);

        // Assert
        result.ShouldBe(1m);
    }

    [Fact]
    public void Compute_WhenShortAndLongRateAreEqual_ReturnsZero()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 1m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 1m);
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
    public void Compute_WhenShortSnapshotIsMissing_ReturnsNull()
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
    public void Compute_WhenLongVolumeIsZero_ReturnsNull()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 5m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 0m);
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
    public void Compute_WhenThresholdIsOne_ReturnsNull()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 3m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 1m);
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
        var result = BuildComputer(threshold: 1m).Compute(context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Compute_WhenShortRateIsBelowLongRate_ReturnsBearish()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 0.5m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 2m);
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
        var result = BuildComputer(threshold: 3.0m).Compute(context);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeLessThan(0m);
    }

    [Fact]
    public void Compute_WhenTimeFrameIsAllTime_ReturnsNull()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 3m);
        var longSnap = SnapshotWithRate(TimeFrame.AllTime, ratePerMinute: 1m);
        var options = Options.Create(
            new SignalOptions
            {
                ShortTimeFrame = TimeFrame.AllTime,
                LongTimeFrame = TimeFrame.AllTime,
            }
        );
        var computer = new VolumeAnomalySignalComputer(options);
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
        var result = computer.Compute(context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Metadata_ShouldExposeExpectedValues()
    {
        // Arrange & Act
        var computer = BuildComputer();

        // Assert
        computer.Type.ShouldBe(SignalType.VolumeAnomaly);
        computer.Label.ShouldBe("Volume Anomaly");
        computer.Description.ShouldNotBeNullOrEmpty();
        computer.Intents.ShouldNotBeEmpty();
    }
}
