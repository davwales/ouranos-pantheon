using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Backtesting.Scorers;

public sealed class TaxAdjustedRoiInputScorerTests
{
    private readonly TaxAdjustedRoiInputScorer _scorer = new();

    private static StrategyScoreContext Context(
        IReadOnlyList<Signal> signals,
        IReadOnlyDictionary<SignalType, IReadOnlyList<decimal>>? history = null
    )
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
            signals,
            StrategyTestFactory.DefaultWeights(),
            new InputThresholds(),
            history
        );
    }

    [Fact]
    public void Score_WhenSignalPresentAndNoHistory_ReturnsLatestSignalValue()
    {
        // Arrange
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TaxAdjustedRoi, 0.6m),
        };

        // Act
        var result = _scorer.Score(Context(signals));

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(0.6m, 0.0001m);
    }

    [Fact]
    public void Score_WhenHistoryPresent_BlendsLatestWithTrendAverage()
    {
        // Arrange
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TaxAdjustedRoi, 0.8m),
        };
        var history = new Dictionary<SignalType, IReadOnlyList<decimal>>
        {
            [SignalType.TaxAdjustedRoi] = [0.2m, 0.3m, 0.4m],
        };

        // Act
        var result = _scorer.Score(Context(signals, history));

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(0.65m, 0.0001m);
    }

    [Fact]
    public void Score_WhenSignalMissing_ReturnsNull()
    {
        // Arrange
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TrendMomentum, 0.5m),
        };

        // Act
        var result = _scorer.Score(Context(signals));

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenSignalValueIsZero_ReturnsNull()
    {
        // Arrange
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TaxAdjustedRoi, 0m),
        };

        // Act
        var result = _scorer.Score(Context(signals));

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Score_WhenHistoryEntryEmpty_FallsBackToLatestValue()
    {
        // Arrange
        var signals = new List<Signal>
        {
            Signal.Create(default, default, SignalType.TaxAdjustedRoi, 0.9m),
        };
        var history = new Dictionary<SignalType, IReadOnlyList<decimal>>
        {
            [SignalType.TaxAdjustedRoi] = [],
        };

        // Act
        var result = _scorer.Score(Context(signals, history));

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(0.9m, 0.0001m);
    }
}
