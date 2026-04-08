using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Signals.Computers;

public sealed class BollingerBandsSignalComputerTests
{
    private static readonly Id<Symbol> SymbolId = new("sym-1");
    private static readonly Id<Market> MarketId = new("mkt-1");

    private static BollingerBandsSignalComputer BuildComputer(int multiplier = 2)
    {
        var options = Options.Create(new SignalOptions { BollingerMultiplier = multiplier });
        return new BollingerBandsSignalComputer(options);
    }

    private static PriceBucket Bucket(decimal avgPrice) =>
        new(DateTimeOffset.UtcNow, avgPrice, avgPrice * 0.99m, avgPrice * 1.01m, 100m);

    [Fact]
    public async Task ComputeAsync_WhenOnlyOneBucket_ReturnsNull()
    {
        // Arrange
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, [Bucket(100m)]);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ComputeAsync_WhenAllPricesEqual_ReturnsZero()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 10).Select(_ => Bucket(100m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer().ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenCurrentPriceIsAtBandMidpoint_ReturnsNeutral()
    {
        // Arrange
        var buckets = new List<PriceBucket>
        {
            Bucket(90m),
            Bucket(110m),
            Bucket(90m),
            Bucket(110m),
            Bucket(90m),
            Bucket(110m),
            Bucket(90m),
            Bucket(110m),
            Bucket(90m),
            Bucket(100m),
        };

        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer(multiplier: 2).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeInRange(-0.1m, 0.1m);
    }

    [Fact]
    public async Task ComputeAsync_WhenCurrentPriceDropsBelowLowerBand_ReturnsBullish()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 9).Select(_ => Bucket(100m)).Append(Bucket(60m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer(multiplier: 2).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenCurrentPriceRisesAboveUpperBand_ReturnsBearish()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 9).Select(_ => Bucket(100m)).Append(Bucket(150m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer(multiplier: 2).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeLessThan(0m);
    }

    [Fact]
    public async Task ComputeAsync_WhenMultiplierIsZero_ReturnsNull()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 9).Select(_ => Bucket(100m)).Append(Bucket(105m)).ToList();
        var context = new SignalComputeContext(SymbolId, MarketId, 0m, 1000m, null, null, null, buckets);

        // Act
        var result = await BuildComputer(multiplier: 0).ComputeAsync(context, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Metadata_ShouldExposeExpectedValues()
    {
        // Arrange & Act
        var computer = BuildComputer();

        // Assert
        computer.Type.ShouldBe(SignalType.BollingerBands);
        computer.Label.ShouldBe("Bollinger Bands");
        computer.Description.ShouldNotBeNullOrEmpty();
        computer.Intents.ShouldNotBeEmpty();
    }
}
