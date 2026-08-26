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
    public async Task ComputeAsync_WhenShortRateEqualsThresholdMultipleOfLongRate_ReturnsOne()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 3m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 1m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = await BuildComputer(threshold: 3.0m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenShortAndLongRateAreEqual_ReturnsZero()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 1m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 1m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenShortSnapshotIsMissing_ReturnsNull()
    {
        // Arrange
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, []);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenLongVolumeIsZero_ReturnsNull()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 5m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 0m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenThresholdIsOne_ReturnsNull()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 3m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 1m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = await BuildComputer(threshold: 1m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenShortRateIsBelowLongRate_ReturnsBearish()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 0.5m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 2m);
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = await BuildComputer(threshold: 3.0m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeLessThan(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenTimeFrameIsAllTime_ReturnsNull()
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
            0m,
            1000m,
            shortSnap,
            null,
            longSnap,
            []
        );

        // Act
        var result = await computer.ComputeAsync(context, CancellationToken.None);

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
