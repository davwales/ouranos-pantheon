using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Backtesting;

public sealed class SignalWeightedExecutorTests
{
    private readonly SignalWeightedExecutor _executor = new();

    private StrategyScoreContext CreateContext(
        IReadOnlyList<Signal> signals,
        SignalWeightedConfig? signalWeightedConfig = null
    )
    {
        return new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            null,
            null,
            null,
            [],
            signals,
            null,
            null,
            SignalWeightedConfig: signalWeightedConfig ?? new SignalWeightedConfig()
        );
    }

    [Fact]
    public void Score_WhenNoSignals_ReturnsNull()
    {
        var context = CreateContext([]);
        var config = new TradingConfiguration();

        var result = _executor.Score(context, config);

        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenNoWeightsConfigured_ReturnsSimpleAverage()
    {
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TaxAdjustedRoi, 0.6m),
            Signal.Create(default, default, SignalType.TrendMomentum, 0.4m),
        };
        var context = CreateContext(signals, new SignalWeightedConfig());

        var result = _executor.Score(context, new TradingConfiguration());

        result.ShouldNotBeNull();
        result.Value.ShouldBe(0.5m, 0.001m);
    }

    [Fact]
    public void Score_WhenWeightsConfigured_ReturnsWeightedAverage()
    {
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.Rsi, 1.0m),
            Signal.Create(default, default, SignalType.TrendMomentum, -1.0m),
        };
        var config = new TradingConfiguration();
        var context = CreateContext(
            signals,
            new SignalWeightedConfig(RsiWeight: 2m, TrendMomentumWeight: 1m)
        );

        var result = _executor.Score(context, config);

        result.ShouldNotBeNull();
        result.Value.ShouldBeInRange(0.3m, 0.35m);
    }

    [Fact]
    public void Score_WhenAllWeightsAreZero_ReturnsNull()
    {
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TaxAdjustedRoi, 0.5m),
        };
        var config = new TradingConfiguration();
        var context = CreateContext(signals, new SignalWeightedConfig(TaxAdjustedRoiWeight: 0m));

        var result = _executor.Score(context, config);

        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenSignalTypeNotInWeightMap_IsIgnored()
    {
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TaxAdjustedRoi, 0.8m),
            Signal.Create(default, default, SignalType.VolumeAnomaly, 0.6m),
        };
        var config = new TradingConfiguration();
        var context = CreateContext(signals, new SignalWeightedConfig(TaxAdjustedRoiWeight: 1m));

        var result = _executor.Score(context, config);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(0.8m, 0.001m);
    }
}

public sealed class ForecastMomentumExecutorTests
{
    private readonly ForecastMomentumExecutor _executor = new();

    [Fact]
    public void Score_WhenNoForecast_ReturnsNull()
    {
        // Arrange
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            null,
            null,
            null,
            [],
            [],
            null,
            null
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenCurrentPriceIsZero_ReturnsNull()
    {
        // Arrange
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            0m,
            0.1m,
            1000m,
            null,
            null,
            null,
            [],
            [],
            null,
            0.05m
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenForecastAtThreshold_ReturnsOneThird()
    {
        // Arrange
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            null,
            null,
            null,
            [],
            [],
            null,
            0.05m,
            ForecastMomentumConfig: new ForecastMomentumConfig(ForecastMovementThreshold: 0.05m)
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeInRange(0.3m, 0.4m);
    }
}

public sealed class MeanReversionExecutorTests
{
    private readonly MeanReversionExecutor _executor = new();

    [Fact]
    public void Score_WhenFewerThanTwoBuckets_ReturnsNull()
    {
        // Arrange
        var buckets = new List<PriceBucket> { new(DateTimeOffset.UtcNow, 100m, 99m, 101m, 10m) };
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            null,
            null,
            null,
            buckets,
            [],
            null,
            null
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenTwoBuckets_ReturnsScore()
    {
        // Arrange
        var buckets = new List<PriceBucket>
        {
            new(DateTimeOffset.UtcNow, 100m, 99m, 101m, 10m),
            new(DateTimeOffset.UtcNow, 102m, 101m, 103m, 10m),
        };
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            90m,
            0.1m,
            1000m,
            null,
            null,
            null,
            buckets,
            [],
            null,
            null
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public void Score_WhenCurrentPriceIsZero_ReturnsNull()
    {
        // Arrange
        var buckets = Enumerable
            .Range(0, 6)
            .Select(_ => new PriceBucket(DateTimeOffset.UtcNow, 100m, 99m, 101m, 10m))
            .ToList();
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            0m,
            0.1m,
            1000m,
            null,
            null,
            null,
            buckets,
            [],
            null,
            null
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenCurrentPriceIsWellBelowMean_ReturnsPositive()
    {
        // Arrange
        var buckets = Enumerable
            .Range(0, 10)
            .Select(i => new PriceBucket(DateTimeOffset.UtcNow, 100m + i, 95m + i, 105m + i, 10m))
            .ToList();
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            90m,
            0.1m,
            1000m,
            null,
            null,
            null,
            buckets,
            [],
            null,
            null,
            MeanReversionConfig: new MeanReversionConfig(DeviationMultiplier: 2m)
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Score_WhenCurrentPriceIsWellAboveMean_ReturnsNegative()
    {
        // Arrange
        var buckets = Enumerable
            .Range(0, 10)
            .Select(i => new PriceBucket(DateTimeOffset.UtcNow, 100m + i, 95m + i, 105m + i, 10m))
            .ToList();
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            110m,
            0.1m,
            1000m,
            null,
            null,
            null,
            buckets,
            [],
            null,
            null,
            MeanReversionConfig: new MeanReversionConfig(DeviationMultiplier: 2m)
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeLessThan(0);
    }

    [Fact]
    public void Score_WhenMeanTimeFrameValueSet_UsesOnlyRecentBuckets()
    {
        // Arrange
        var earlyBuckets = Enumerable
            .Range(0, 5)
            .Select(i => new PriceBucket(DateTimeOffset.UtcNow, 200m + i, 195m + i, 205m + i, 10m))
            .ToList();
        var recentBuckets = Enumerable
            .Range(0, 5)
            .Select(i => new PriceBucket(DateTimeOffset.UtcNow, 100m + i, 95m + i, 105m + i, 10m))
            .ToList();
        var allBuckets = earlyBuckets.Concat(recentBuckets).ToList();

        var contextAll = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            90m,
            0.1m,
            1000m,
            null,
            null,
            null,
            allBuckets,
            [],
            null,
            null
        );

        // Act
        var resultAllBuckets = _executor.Score(contextAll, new TradingConfiguration());
        var resultRecentOnly = _executor.Score(
            contextAll with
            {
                MeanReversionConfig = new MeanReversionConfig(
                    DeviationMultiplier: 2m,
                    MeanTimeFrameValue: 5
                ),
            },
            new TradingConfiguration()
        );

        // Assert
        resultAllBuckets.ShouldNotBeNull();
        resultRecentOnly.ShouldNotBeNull();
        resultAllBuckets.Value.ShouldNotBe(resultRecentOnly.Value);
    }
}

public sealed class RecipeArbitrageExecutorTests
{
    private readonly RecipeArbitrageExecutor _executor = new();

    [Fact]
    public void Score_WhenNoSnapshot_ReturnsNull()
    {
        // Arrange
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            null,
            null,
            null,
            [],
            [],
            null,
            null
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenRoiExceedsMinMargin_ReturnsPositive()
    {
        // Arrange
        var snap = MarketTradeSnapshot.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            new Id<Symbol>(Guid.NewGuid().ToString()),
            TimeFrame.OneHour,
            10000m,
            90m,
            100m,
            1000m,
            500,
            1000m,
            0.05m
        );
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            snap,
            null,
            null,
            [],
            [],
            null,
            null,
            RecipeArbitrageConfig: new RecipeArbitrageConfig(MinMarginPercent: 0.01m)
        );

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThan(0);
    }
}

public sealed class CompositeExecutorTests
{
    [Fact]
    public void Score_WhenNoComponents_ReturnsNull()
    {
        // Arrange
        var executor = new CompositeExecutor([new SignalWeightedExecutor()]);
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            null,
            null,
            null,
            [],
            [],
            null,
            null,
            Components: null
        );

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenComponentTypeHasNoMatchingExecutor_SkipsComponent()
    {
        // Arrange
        var executor = new CompositeExecutor([new SignalWeightedExecutor()]);
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            100m,
            0.1m,
            1000m,
            null,
            null,
            null,
            [],
            [],
            null,
            null,
            Components:
            [
                new CompositeComponent(
                    new Id<Strategy>(Guid.NewGuid().ToString()),
                    StrategyType.ForecastMomentum,
                    1m
                ),
            ]
        );

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_ResultIsClamped()
    {
        // Arrange
        var executor = new CompositeExecutor([new MeanReversionExecutor()]);
        var buckets = Enumerable
            .Range(0, 10)
            .Select(_ => new PriceBucket(DateTimeOffset.UtcNow, 100m, 95m, 105m, 10m))
            .ToList();
        var context = new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            50m,
            0.1m,
            1000m,
            null,
            null,
            null,
            buckets,
            [],
            null,
            null,
            MeanReversionConfig: new MeanReversionConfig(DeviationMultiplier: 0.01m),
            Components:
            [
                new CompositeComponent(
                    new Id<Strategy>(Guid.NewGuid().ToString()),
                    StrategyType.MeanReversion,
                    100m
                ),
            ]
        );

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeGreaterThanOrEqualTo(-1m);
        result.Value.ShouldBeLessThanOrEqualTo(1m);
    }
}
