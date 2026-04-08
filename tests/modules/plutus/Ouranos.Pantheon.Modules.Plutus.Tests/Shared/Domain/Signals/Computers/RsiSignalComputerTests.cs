using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Signals.Computers;

public sealed class RsiSignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static RsiSignalComputer BuildComputer() => new();

    private static PriceBucket Bucket(decimal avgPrice) =>
        new(DateTimeOffset.UtcNow, avgPrice, avgPrice * 0.99m, avgPrice * 1.01m, 100m);

    [Fact]
    public async Task ComputeAsync_WhenFewerBucketsThanRsiPeriod_ReturnsNull()
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
    public async Task ComputeAsync_WhenPricesRiseConsistently_ReturnsBearish()
    {
        // Arrange
        var buckets = Enumerable.Range(1, 20).Select(i => Bucket(i * 10m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeLessThan(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenPricesFallConsistently_ReturnsBullish()
    {
        // Arrange
        var buckets = Enumerable.Range(1, 20).Select(i => Bucket(200m - i * 10m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task ComputeAsync_ResultIsAlwaysWithinBounds()
    {
        // Arrange
        var buckets = Enumerable.Range(1, 30).Select(i => Bucket(i * 5m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeInRange(-1m, 1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenNoLosses_ReturnsMaxBearish()
    {
        // Arrange
        var buckets = Enumerable.Range(1, 20).Select(i => Bucket(i * 10m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(-1m);
    }

    [Fact]
    public void Metadata_ShouldExposeExpectedValues()
    {
        // Arrange & Act
        var computer = BuildComputer();

        // Assert
        computer.Type.ShouldBe(SignalType.Rsi);
        computer.Label.ShouldBe("RSI");
        computer.Description.ShouldNotBeNullOrEmpty();
        computer.Intents.ShouldNotBeEmpty();
    }
}
