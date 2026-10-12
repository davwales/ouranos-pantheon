using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Swing;

public sealed class SwingScoringTests
{
    private static readonly DateOnly Day0 = SwingTestData.Day(SwingTestData.Start);

    private readonly Symbol _symbol = SwingTestData.CreateSymbol(SwingTestData.CreateMarket(), "A");

    [Theory]
    [InlineData(InputKind.SignalTaxAdjustedRoi, SignalType.TaxAdjustedRoi)]
    [InlineData(InputKind.SignalVolumeAnomaly, SignalType.VolumeAnomaly)]
    [InlineData(InputKind.SignalTrendMomentum, SignalType.TrendMomentum)]
    [InlineData(InputKind.SignalBollingerBands, SignalType.BollingerBands)]
    [InlineData(InputKind.SignalRsi, SignalType.Rsi)]
    [InlineData(InputKind.SignalMovingAverageCrossover, SignalType.MovingAverageCrossover)]
    [InlineData(InputKind.SignalPriceVelocity, SignalType.PriceVelocity)]
    public void SignalOf_ShouldMapEveryInputKindToItsSignal(InputKind kind, SignalType expected)
    {
        // Act
        var signal = SwingScoring.SignalOf(kind);

        // Assert
        signal.ShouldBe(expected);
    }

    [Fact]
    public void SignalOf_ShouldCoverEveryInputKind()
    {
        // Act
        var signals = Enum.GetValues<InputKind>().Select(SwingScoring.SignalOf).ToList();

        // Assert
        signals.ShouldBeUnique();
        signals.Count.ShouldBe(Enum.GetValues<SignalType>().Length);
    }

    [Fact]
    public void SignalOf_WhenKindIsUnknown_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => SwingScoring.SignalOf((InputKind)99));
    }

    [Fact]
    public void Combine_ShouldNormalizeByTotalAbsoluteWeight()
    {
        // Arrange
        var scores = new Dictionary<InputKind, decimal>
        {
            [InputKind.SignalTaxAdjustedRoi] = 0.6m,
            [InputKind.SignalRsi] = 0.3m,
        };

        InputWeight[] weights =
        [
            new(InputKind.SignalTaxAdjustedRoi, 2m),
            new(InputKind.SignalRsi, 1m),
        ];

        // Act
        var score = SwingScoring.Combine(scores, weights);

        // Assert
        score.ShouldBe(0.5m);
    }

    [Fact]
    public void Combine_WhenWeightIsNegative_ShouldInvertThatInputOnly()
    {
        // Arrange
        var scores = new Dictionary<InputKind, decimal>
        {
            [InputKind.SignalTaxAdjustedRoi] = 0.8m,
            [InputKind.SignalRsi] = -0.4m,
        };

        InputWeight[] weights =
        [
            new(InputKind.SignalTaxAdjustedRoi, 1m),
            new(InputKind.SignalRsi, -1m),
        ];

        // Act
        var score = SwingScoring.Combine(scores, weights);

        // Assert
        score.ShouldBe(0.6m);
    }

    [Fact]
    public void Combine_ShouldIgnoreZeroWeightsAndInputsWithoutAScore()
    {
        // Arrange
        var scores = new Dictionary<InputKind, decimal>
        {
            [InputKind.SignalTaxAdjustedRoi] = 0.4m,
            [InputKind.SignalVolumeAnomaly] = -1m,
        };

        InputWeight[] weights =
        [
            new(InputKind.SignalTaxAdjustedRoi, 1m),
            new(InputKind.SignalVolumeAnomaly, 0m),
            new(InputKind.SignalRsi, 5m),
        ];

        // Act
        var score = SwingScoring.Combine(scores, weights);

        // Assert
        score.ShouldBe(0.4m);
    }

    [Theory]
    [InlineData(3, 1)]
    [InlineData(-3, -1)]
    public void Combine_ShouldClampToUnitRange(decimal input, decimal expected)
    {
        // Arrange
        var scores = new Dictionary<InputKind, decimal> { [InputKind.SignalRsi] = input };

        // Act
        var score = SwingScoring.Combine(scores, [new InputWeight(InputKind.SignalRsi, 1m)]);

        // Assert
        score.ShouldBe(expected);
    }

    [Fact]
    public void Combine_WhenNoWeightedInputHasAScore_ShouldReturnNull()
    {
        // Arrange
        var scores = new Dictionary<InputKind, decimal> { [InputKind.SignalRsi] = 0.5m };

        // Act
        var score = SwingScoring.Combine(
            scores,
            [
                new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m),
                new InputWeight(InputKind.SignalRsi, 0m),
            ]
        );

        // Assert
        score.ShouldBeNull();
    }

    [Fact]
    public void ScoreInputs_ShouldBlendTheLatestSignalWithTheAverageOfPreviousEligibleDays()
    {
        // Arrange
        var table = Table(
            SwingTestData.Signal(0.5m),
            SwingTestData.Signal(1.0m),
            SwingTestData.Signal(0.2m, isEligible: false),
            SwingTestData.Signal(0.4m)
        );

        // Act
        var inputs = SwingScoring.ScoreInputs(table, Day0, Day0.AddDays(3));

        // Assert
        Score(inputs, 0).ShouldBe(0.5m);
        Score(inputs, 1).ShouldBe(0.85m);
        Score(inputs, 2).ShouldBe(0.365m);
        Score(inputs, 3).ShouldBe(0.505m);
    }

    [Fact]
    public void ScoreInputs_ShouldStartWithEmptyHistoryAtTheFirstDay()
    {
        // Arrange
        var table = Table(SwingTestData.Signal(0.5m), SwingTestData.Signal(1.0m));

        // Act
        var inputs = SwingScoring.ScoreInputs(table, Day0.AddDays(1), Day0.AddDays(1));

        // Assert
        inputs.Keys.ShouldBe([Day0.AddDays(1)]);
        Score(inputs, 1).ShouldBe(1.0m);
    }

    [Fact]
    public void ScoreInputs_ShouldOnlyAverageTheLastSixEligibleDays()
    {
        // Arrange
        var table = Table([
            SwingTestData.Signal(-1m),
            .. Enumerable.Repeat(
                SwingTestData.Signal(1m),
                SwingScoring.SignalHistoryWindowSize + 1
            ),
        ]);

        // Act
        var inputs = SwingScoring.ScoreInputs(table, Day0, Day0.AddDays(7));

        // Assert
        Score(inputs, 6).ShouldBeLessThan(1m);
        Score(inputs, 7).ShouldBe(1m);
    }

    [Fact]
    public void ScoreInputs_ShouldCarryPriceAndEligibilityAndSkipZeroSignals()
    {
        // Arrange
        var table = Table(
            new SwingSignalDay(
                42m,
                false,
                new Dictionary<SignalType, decimal>
                {
                    [SignalType.Rsi] = 0.3m,
                    [SignalType.PriceVelocity] = 0m,
                }
            )
        );

        // Act
        var inputs = SwingScoring.ScoreInputs(table, Day0, Day0);

        // Assert
        var symbolInputs = inputs[Day0][_symbol.Id];
        symbolInputs.Price.ShouldBe(42m);
        symbolInputs.IsEligible.ShouldBeFalse();
        symbolInputs.Scores.ShouldBe(
            new Dictionary<InputKind, decimal> { [InputKind.SignalRsi] = 0.3m }
        );
    }

    [Fact]
    public void ScoreInputs_WhenADayHasNoSignals_ShouldReturnAnEmptyDay()
    {
        // Arrange
        var table = Table(SwingTestData.Signal(0.5m));

        // Act
        var inputs = SwingScoring.ScoreInputs(table, Day0, Day0.AddDays(1));

        // Assert
        inputs[Day0.AddDays(1)].ShouldBeEmpty();
    }

    private SwingSignalTable Table(params SwingSignalDay[] days)
    {
        return SwingTestData.Signals(days.Length - 1, [_symbol], (day, _) => days[day]);
    }

    private decimal Score(
        IReadOnlyDictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingInputs>> inputs,
        int day
    )
    {
        return inputs[Day0.AddDays(day)][_symbol.Id].Scores[InputKind.SignalTaxAdjustedRoi];
    }
}
