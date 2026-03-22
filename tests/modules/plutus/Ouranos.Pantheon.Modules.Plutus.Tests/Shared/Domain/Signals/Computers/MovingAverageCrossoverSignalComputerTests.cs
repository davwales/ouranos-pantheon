using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Signals.Computers;

public sealed class MovingAverageCrossoverSignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static MovingAverageCrossoverSignalComputer BuildComputer(decimal threshold = 0.02m)
    {
        var options = Options.Create(new SignalOptions { MaCrossoverThreshold = threshold });
        return new MovingAverageCrossoverSignalComputer(options);
    }

    private static PriceBucket Bucket(decimal avgPrice) =>
        new(DateTimeOffset.UtcNow, avgPrice, avgPrice * 0.99m, avgPrice * 1.01m, 100m);

    [Fact]
    public async Task ComputeAsync_WhenFewerBucketsThanLongPeriod_ReturnsNull()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 10).Select(_ => Bucket(100m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenAllPricesEqual_ReturnsZero()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 25).Select(_ => Bucket(100m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenShortMaIsAboveLongMa_ReturnsBullish()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 15).Select(_ => Bucket(100m))
            .Concat(Enumerable.Range(0, 5).Select(_ => Bucket(102m)))
            .ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer(threshold: 0.02m).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenShortMaIsBelowLongMa_ReturnsBearish()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 15).Select(_ => Bucket(100m))
            .Concat(Enumerable.Range(0, 5).Select(_ => Bucket(98m)))
            .ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer(threshold: 0.02m).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeLessThan(0m);
    }
}
