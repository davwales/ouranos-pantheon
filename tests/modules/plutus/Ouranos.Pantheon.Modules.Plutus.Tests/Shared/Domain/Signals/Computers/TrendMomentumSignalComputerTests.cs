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

public sealed class TrendMomentumSignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static TrendMomentumSignalComputer BuildComputer(decimal threshold = 0.05m)
    {
        var options = Options.Create(new SignalOptions { MomentumThreshold = threshold });
        return new TrendMomentumSignalComputer(options);
    }

    private static MarketTradeSnapshot MakeSnapshot(TimeFrame frame, decimal avgPrice, decimal volume = 1000m)
        => MarketTradeSnapshot.Create(
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

    [Fact]
    public async Task ComputeAsync_WhenShortAverageDeltaEqualsThreshold_ReturnsOne()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 105m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

        // Act
        var result = await BuildComputer(threshold: 0.05m).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenShortAverageIsLessThanLong_ReturnsBearish()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 95m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

        // Act
        var result = await BuildComputer(threshold: 0.05m).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(-1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenShortAndLongAveragesAreEqual_ReturnsZero()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 100m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenSnapshotsAreMissing_ReturnsNull()
    {
        // Arrange
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, []);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenMomentumThresholdIsZero_ReturnsNull()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 105m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m);
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

        // Act
        var result = await BuildComputer(threshold: 0m).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenLongSnapshotHasZeroVolume_ReturnsNull()
    {
        // Arrange
        var shortSnap = MakeSnapshot(TimeFrame.OneHour, avgPrice: 105m, volume: 1000m);
        var longSnap = MakeSnapshot(TimeFrame.OneMonth, avgPrice: 100m, volume: 0m);
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, shortSnap, null, longSnap, []);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

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
