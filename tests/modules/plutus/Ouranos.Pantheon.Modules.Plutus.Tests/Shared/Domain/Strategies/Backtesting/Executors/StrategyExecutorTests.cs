using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Backtesting.Executors;

public sealed class StrategyExecutorTests
{
    private readonly StrategyExecutor _executor = new([]);

    private static StrategyScoreContext ContextWithWeights(IReadOnlyList<InputWeight> weights)
    {
        return new StrategyScoreContext(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            "TEST",
            null,
            100m,
            0m,
            decimal.MaxValue,
            null,
            null,
            null,
            [],
            [],
            weights,
            new InputThresholds()
        );
    }

    /// <summary>
    ///     Deterministic stub scorer that returns a fixed score for one input kind,
    ///     and null for every other kind. Lets the executor tests assert exact blends
    ///     without depending on the real scorers' data setup.
    /// </summary>
    private sealed class StubScorer(InputKind kind, decimal? score) : IInputScorer
    {
        public InputKind Kind { get; } = kind;

        public decimal? Score(StrategyScoreContext context)
        {
            return score;
        }
    }

    [Fact]
    public void Score_WhenNoWeights_ReturnsNull()
    {
        // Arrange
        var context = ContextWithWeights([new InputWeight(InputKind.SignalTaxAdjustedRoi, 0m)]);

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenAllWeightsAreZero_ReturnsNull()
    {
        // Arrange
        var context = ContextWithWeights([
            new InputWeight(InputKind.SignalTaxAdjustedRoi, 0m),
            new InputWeight(InputKind.SignalTrendMomentum, 0m),
        ]);

        // Act
        var result = _executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenWeightedKindHasNoScorer_ReturnsNull()
    {
        // Arrange
        var context = ContextWithWeights([new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m)]);
        var executor = new StrategyExecutor([]);

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenTwoScorersWithKnownScores_ReturnsWeightedAverage()
    {
        // Arrange
        var context = ContextWithWeights([
            new InputWeight(InputKind.SignalTaxAdjustedRoi, 3m),
            new InputWeight(InputKind.SignalTrendMomentum, 1m),
        ]);
        var executor = new StrategyExecutor([
            new StubScorer(InputKind.SignalTaxAdjustedRoi, 0.8m),
            new StubScorer(InputKind.SignalTrendMomentum, 0.4m),
        ]);

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(0.7m, 0.0001m);
    }

    [Fact]
    public void Score_WhenScorerReturnsNull_IsSkippedFromBlend()
    {
        // Arrange
        var context = ContextWithWeights([
            new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m),
            new InputWeight(InputKind.SignalTrendMomentum, 1m),
        ]);
        var executor = new StrategyExecutor([
            new StubScorer(InputKind.SignalTaxAdjustedRoi, 0.6m),
            new StubScorer(InputKind.SignalTrendMomentum, null),
        ]);

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(0.6m, 0.0001m);
    }

    [Fact]
    public void Score_WhenBlendExceedsOne_IsClampedToOne()
    {
        // Arrange
        var context = ContextWithWeights([new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m)]);
        var executor = new StrategyExecutor([new StubScorer(InputKind.SignalTaxAdjustedRoi, 5m)]);

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(1m);
    }

    [Fact]
    public void Score_WhenBlendBelowNegativeOne_IsClampedToNegativeOne()
    {
        // Arrange
        var context = ContextWithWeights([new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m)]);
        var executor = new StrategyExecutor([new StubScorer(InputKind.SignalTaxAdjustedRoi, -5m)]);

        // Act
        var result = executor.Score(context, new TradingConfiguration());

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(-1m);
    }
}
