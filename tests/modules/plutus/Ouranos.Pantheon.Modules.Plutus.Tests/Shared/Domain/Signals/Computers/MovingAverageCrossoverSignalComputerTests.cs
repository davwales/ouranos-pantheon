using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

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

    private static PriceBucket Bucket(decimal avgPrice)
    {
        return new(DateTimeOffset.UtcNow, avgPrice, avgPrice * 0.99m, avgPrice * 1.01m, 100m);
    }

    [Fact]
    public void Compute_WhenFewerBucketsThanLongPeriod_ReturnsNull()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 10).Select(_ => Bucket(100m)).ToList();
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = BuildComputer().Compute(context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Compute_WhenAllPricesEqual_ReturnsZero()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 25).Select(_ => Bucket(100m)).ToList();
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = BuildComputer().Compute(context);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void Compute_WhenShortMaIsAboveLongMa_ReturnsBullish()
    {
        // Arrange
        var buckets = Enumerable
            .Range(0, 15)
            .Select(_ => Bucket(100m))
            .Concat(Enumerable.Range(0, 5).Select(_ => Bucket(102m)))
            .ToList();
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = BuildComputer(threshold: 0.02m).Compute(context);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public void Compute_WhenShortMaIsBelowLongMa_ReturnsBearish()
    {
        // Arrange
        var buckets = Enumerable
            .Range(0, 15)
            .Select(_ => Bucket(100m))
            .Concat(Enumerable.Range(0, 5).Select(_ => Bucket(98m)))
            .ToList();
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = BuildComputer(threshold: 0.02m).Compute(context);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeLessThan(0m);
    }

    [Fact]
    public void Compute_WhenThresholdIsZero_ReturnsNull()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 25).Select(_ => Bucket(100m)).ToList();
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            null,
            null,
            null,
            buckets
        );

        // Act
        var result = BuildComputer(threshold: 0m).Compute(context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Compute_WhenSma20IsZero_ReturnsNull()
    {
        // Arrange
        var buckets = Enumerable.Range(0, 25).Select(_ => Bucket(0m)).ToList();
        var context = new SignalComputeContext(
            SymbolId,
            MarketId,
            MarketRules.TaxFree,
            1000m,
            null,
            null,
            null,
            buckets
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
        computer.Type.ShouldBe(SignalType.MovingAverageCrossover);
        computer.Label.ShouldBe("Moving Average Crossover");
        computer.Description.ShouldNotBeNullOrEmpty();
        computer.Intents.ShouldNotBeEmpty();
    }
}
