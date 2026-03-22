using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

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

        return MarketTradeSnapshot.Create(
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
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

        // Act
        var result = await BuildComputer(threshold: 3.0m).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenShortAndLongRateAreEqual_ReturnsZero()
    {
        // Arrange
        var shortSnap = SnapshotWithRate(TimeFrame.OneHour, ratePerMinute: 1m);
        var longSnap = SnapshotWithRate(TimeFrame.OneMonth, ratePerMinute: 1m);
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

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
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}
