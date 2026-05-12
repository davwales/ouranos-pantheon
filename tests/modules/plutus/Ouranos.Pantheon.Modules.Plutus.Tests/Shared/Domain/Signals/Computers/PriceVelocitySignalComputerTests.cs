using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Signals.Computers;

public sealed class PriceVelocitySignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static PriceVelocitySignalComputer BuildComputer(decimal threshold = 0.03m)
    {
        var options = Options.Create(new SignalOptions { PriceVelocityThreshold = threshold });
        return new PriceVelocitySignalComputer(options);
    }

    private static PriceBucket Bucket(decimal avgPrice) =>
        new(DateTimeOffset.UtcNow, avgPrice, avgPrice * 0.99m, avgPrice * 1.01m, 100m);

    [Fact]
    public async Task ComputeAsync_WhenOnlyOneBucket_ReturnsNull()
    {
        // Arrange
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            null,
            null,
            null,
            [Bucket(100m)]
        );

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenFirstPriceIsZero_ReturnsNull()
    {
        // Arrange
        var buckets = new List<PriceBucket> { Bucket(0m), Bucket(100m) };
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenPriceIsUnchanged_ReturnsZero()
    {
        // Arrange
        var buckets = new List<PriceBucket> { Bucket(100m), Bucket(100m) };
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenPriceRiseEqualsThreshold_ReturnsOne()
    {
        // Arrange
        var buckets = new List<PriceBucket> { Bucket(100m), Bucket(103m) };
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = await BuildComputer(threshold: 0.03m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenPriceFallEqualsThreshold_ReturnsNegativeOne()
    {
        // Arrange
        var buckets = new List<PriceBucket> { Bucket(100m), Bucket(97m) };
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = await BuildComputer(threshold: 0.03m)
            .ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(-1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenThresholdIsZero_ReturnsNull()
    {
        // Arrange
        var buckets = new List<PriceBucket> { Bucket(100m), Bucket(103m) };
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            0m,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = await BuildComputer(threshold: 0m)
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
        computer.Type.ShouldBe(SignalType.PriceVelocity);
        computer.Label.ShouldBe("Price Velocity");
        computer.Description.ShouldNotBeNullOrEmpty();
        computer.Intents.ShouldNotBeEmpty();
    }
}
