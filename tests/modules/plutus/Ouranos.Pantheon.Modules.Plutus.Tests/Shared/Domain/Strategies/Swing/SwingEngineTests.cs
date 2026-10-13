using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Swing;

public sealed class SwingEngineTests
{
    private readonly ISwingDataQueryService _dataService = Substitute.For<ISwingDataQueryService>();
    private readonly ISignalComputer _computer = Substitute.For<ISignalComputer>();
    private readonly Market _market = SwingTestData.CreateMarket();
    private readonly Dictionary<Id<Symbol>, decimal> _scores = [];
    private readonly IStrategyEngine _engine;

    public SwingEngineTests()
    {
        _computer.Type.Returns(SignalType.TaxAdjustedRoi);

        _computer
            .ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
                Task.FromResult<decimal?>(
                    _scores.TryGetValue(call.Arg<SignalComputeContext>().SymbolId, out var score)
                        ? score
                        : null
                )
            );

        _engine = new SwingEngine(
            _dataService,
            new SwingSignalReconstructor([_computer]),
            Options.Create(new BacktestDataOptions())
        );
    }

    [Fact]
    public void Mode_ShouldBeSwing()
    {
        // Act
        var mode = _engine.Mode;

        // Assert
        mode.ShouldBe(StrategyMode.Swing);
    }

    [Fact]
    public void CreateParameterSpace_ShouldTuneSizingThenActiveWeightsInEnumOrderThenThresholds()
    {
        // Arrange
        var seed = new SwingConfiguration([
            new InputWeight(InputKind.SignalRsi, 0.5m),
            new InputWeight(InputKind.SignalTaxAdjustedRoi, 1m),
        ]);

        // Act
        var space = _engine.CreateParameterSpace(seed);

        // Assert
        space
            .Parameters.Select(p => p.Key)
            .ShouldBe([
                "maxPositions",
                "maxPositionPercent",
                "holdPeriodDays",
                "weight.SignalTaxAdjustedRoi",
                "weight.SignalRsi",
                "buyThreshold",
                "sellThreshold",
            ]);

        space.Parameters[0].IsWhole.ShouldBeTrue();
        space.Parameters[2].IsWhole.ShouldBeTrue();
        space.Parameters[3].Min.ShouldBe(-3);
        space.Parameters[3].Max.ShouldBe(3);
    }

    [Fact]
    public void CreateParameterSpace_WhenSeedHasNoLimits_ShouldStartFromTheLoosestValues()
    {
        // Arrange
        var seed = new SwingConfiguration([new InputWeight(InputKind.SignalRsi, 5m)]);

        // Act
        var space = _engine.CreateParameterSpace(seed);

        // Assert
        space.Seed.ShouldBe([60d, 0.15d, 60d, 3d, 0d, 0d]);
    }

    [Fact]
    public void CreateParameterSpace_ShouldClampSeedValuesIntoRange()
    {
        // Arrange
        var seed = new SwingConfiguration(
            [new InputWeight(InputKind.SignalRsi, -5m)],
            BuyThreshold: 2m,
            SellThreshold: -2m,
            MaxPositions: 3,
            MaxPositionPercent: 0.001m,
            HoldPeriodDays: 100
        );

        // Act
        var space = _engine.CreateParameterSpace(seed);

        // Assert
        space.Seed.ShouldBe([8d, 0.01d, 60d, -3d, 0.95d, -0.9d]);
    }

    [Fact]
    public void CreateParameterSpace_WhenBuildingTheSeed_ShouldRoundTripTheConfiguration()
    {
        // Arrange
        var seed = new SwingConfiguration(
            [
                new InputWeight(InputKind.SignalRsi, -0.5m),
                new InputWeight(InputKind.SignalTaxAdjustedRoi, 1.25m),
            ],
            BuyThreshold: 0.2m,
            SellThreshold: -0.1m,
            MaxPositions: 10,
            MaxPositionPercent: 0.05m,
            HoldPeriodDays: 7
        );

        var space = _engine.CreateParameterSpace(seed);

        // Act
        var built = space.Build(space.Seed).ShouldBeOfType<SwingConfiguration>();

        // Assert
        built.InputWeights.ShouldBe([
            new InputWeight(InputKind.SignalTaxAdjustedRoi, 1.25m),
            new InputWeight(InputKind.SignalRsi, -0.5m),
        ]);

        built.BuyThreshold.ShouldBe(0.2m);
        built.SellThreshold.ShouldBe(-0.1m);
        built.MaxPositions.ShouldBe(10);
        built.MaxPositionPercent.ShouldBe(0.05m);
        built.HoldPeriodDays.ShouldBe(7);
    }

    [Fact]
    public void CreateParameterSpace_WhenBuildingNewGenes_ShouldOnlyWeightTheSeedsKinds()
    {
        // Arrange
        var seed = new SwingConfiguration([new InputWeight(InputKind.SignalBollingerBands, 1m)]);
        var space = _engine.CreateParameterSpace(seed);

        // Act
        var built = space
            .Build([12d, 0.1d, 5d, -2d, 0.4d, -0.3d])
            .ShouldBeOfType<SwingConfiguration>();

        // Assert
        built.InputWeights.ShouldBe([new InputWeight(InputKind.SignalBollingerBands, -2m)]);
        built.MaxPositions.ShouldBe(12);
        built.MaxPositionPercent.ShouldBe(0.1m);
        built.HoldPeriodDays.ShouldBe(5);
        built.BuyThreshold.ShouldBe(0.4m);
        built.SellThreshold.ShouldBe(-0.3m);
    }

    [Fact]
    public void CreateParameterSpace_WhenSeedIsFlip_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            _engine.CreateParameterSpace(new FlipConfiguration())
        );
    }

    [Fact]
    public void Simulate_WhenConfigurationIsFlip_ShouldThrowArgumentException()
    {
        // Arrange
        var data = new SwingData(
            SwingMarketData.FromRaw(_market, [], []),
            SwingTestData.Signals(0, [], (_, _) => null)
        );

        var settings = new SimulationSettings(
            SwingTestData.Start,
            SwingTestData.Start.AddDays(1),
            1000m,
            0.1m,
            0m
        );

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            _engine.Simulate(data, new FlipConfiguration(), settings)
        );
    }

    [Fact]
    public void Simulate_WhenDataIsNotSwingData_ShouldThrowArgumentException()
    {
        // Arrange
        var data = new FlipMarketData(SwingTestData.Start, 0, [], MarketRules.TaxFree);

        var settings = new SimulationSettings(
            SwingTestData.Start,
            SwingTestData.Start.AddDays(1),
            1000m,
            0.1m,
            0m
        );

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            _engine.Simulate(data, StrategyTestFactory.Swing(), settings)
        );
    }

    [Fact]
    public void Simulate_WhenNothingTrades_ShouldKeepTheBudget()
    {
        // Arrange
        var data = new SwingData(
            SwingMarketData.FromRaw(_market, [], []),
            SwingTestData.Signals(2, [], (_, _) => null)
        );

        var settings = new SimulationSettings(
            SwingTestData.Start,
            SwingTestData.Start.AddDays(2),
            1000m,
            0.1m,
            0m
        );

        // Act
        var result = _engine.Simulate(data, StrategyTestFactory.Swing(), settings);

        // Assert
        result.Positions.ShouldBeEmpty();
        result.FinalBalance.ShouldBe(1000m);
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadLookbackHistoryAndReconstructSignals()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        _scores[a.Id] = 0.5m;
        var start = SwingTestData.Start;
        var end = start.AddDays(2);

        _dataService
            .LoadDataAsync(_market.Id, start, end, 30, Arg.Any<CancellationToken>())
            .Returns(
                SwingMarketData.FromRaw(_market, [a], [.. SwingTestData.Steady(a, 2, 10m, 100m)])
            );

        // Act
        var data = await _engine.LoadAsync(
            _market.Id,
            start,
            end,
            StrategyTestFactory.Swing(),
            CancellationToken.None
        );

        // Assert
        var swing = data.ShouldBeOfType<SwingData>();

        swing
            .Signals.On(SwingTestData.Day(end))[a.Id]
            .Signals[SignalType.TaxAdjustedRoi]
            .ShouldBe(0.5m);

        await _dataService
            .Received(1)
            .LoadDataAsync(_market.Id, start, end, 30, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecommendAsync_ShouldBuyTheBestItemsAboveTheThresholdWithinBudget()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var b = SwingTestData.CreateSymbol(_market, "B");
        var c = SwingTestData.CreateSymbol(_market, "C");
        Symbol[] symbols = [c, b, a];
        _scores[a.Id] = 0.9m;
        _scores[b.Id] = 0.6m;
        _scores[c.Id] = 0.2m;
        ArrangeRecentHistory(symbols);

        var configuration = new SwingConfiguration(
            StrategyTestFactory.DefaultWeights(),
            BuyThreshold: 0.3m,
            MaxPositionPercent: 0.5m
        );

        // Act
        var recommendations = await _engine.RecommendAsync(
            _market.Id,
            configuration,
            1000m,
            CancellationToken.None
        );

        // Assert
        recommendations.Select(r => r.SymbolName).ShouldBe(["A", "B"]);
        recommendations[0].Score.ShouldBe(0.9m, 0.0001m);
        recommendations[0].CurrentPrice.ShouldBe(10m);
        recommendations[0].SuggestedVolume.ShouldBe(50m);
        recommendations[0].SuggestedAllocation.ShouldBe(500m);
        recommendations[0].TypicalDailyVolume.ShouldBe(100_000m);
        recommendations[0].LimitedBy.ShouldBe(PositionLimit.Budget);
        recommendations[0].SymbolId.ShouldBe(a.Id.ToString());
        recommendations[1].SuggestedVolume.ShouldBe(25m);
        recommendations.Sum(r => r.SuggestedAllocation).ShouldBeLessThanOrEqualTo(1000m);
    }

    [Fact]
    public async Task RecommendAsync_ShouldStopAtMaxPositions()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");
        var b = SwingTestData.CreateSymbol(_market, "B");
        _scores[a.Id] = 0.9m;
        _scores[b.Id] = 0.6m;
        ArrangeRecentHistory([a, b]);

        var configuration = new SwingConfiguration(
            StrategyTestFactory.DefaultWeights(),
            MaxPositions: 1,
            MaxPositionPercent: 0.5m
        );

        // Act
        var recommendations = await _engine.RecommendAsync(
            _market.Id,
            configuration,
            1000m,
            CancellationToken.None
        );

        // Assert
        recommendations.ShouldHaveSingleItem().SymbolName.ShouldBe("A");
    }

    [Fact]
    public async Task RecommendAsync_WhenSymbolHasBuyLimit_ShouldBeLimitedByIt()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A", limit: 5m);
        _scores[a.Id] = 0.9m;
        ArrangeRecentHistory([a]);

        // Act
        var recommendations = await _engine.RecommendAsync(
            _market.Id,
            StrategyTestFactory.Swing(),
            1000m,
            CancellationToken.None
        );

        // Assert
        var recommendation = recommendations.ShouldHaveSingleItem();
        recommendation.SuggestedVolume.ShouldBe(10m);
        recommendation.LimitedBy.ShouldBe(PositionLimit.BuyLimit);
    }

    [Fact]
    public async Task RecommendAsync_WhenNothingIsEligible_ShouldReturnNothing()
    {
        // Arrange
        _dataService
            .LoadDataAsync(
                _market.Id,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(SwingMarketData.FromRaw(_market, [], []));

        // Act
        var recommendations = await _engine.RecommendAsync(
            _market.Id,
            StrategyTestFactory.Swing(),
            1000m,
            CancellationToken.None
        );

        // Assert
        recommendations.ShouldBeEmpty();
    }

    private void ArrangeRecentHistory(IReadOnlyList<Symbol> symbols)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _dataService
            .LoadDataAsync(
                _market.Id,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                SwingMarketData.FromRaw(
                    _market,
                    [.. symbols],
                    [
                        .. symbols.SelectMany(s =>
                            SwingTestData.Steady(s, today.AddDays(-20), today, 10m, 100_000m)
                        ),
                    ]
                )
            );
    }

    [Fact]
    public void CreateParameterSpace_WhenParametersAreLocked_ShouldKeepThemAtTheSeedValue()
    {
        // Arrange
        var seed = new SwingConfiguration(
            [
                new InputWeight(InputKind.SignalRsi, -0.5m),
                new InputWeight(InputKind.SignalTaxAdjustedRoi, 1.25m),
            ],
            MaxPositions: 4
        ) with
        {
            LockedParameters = ["maxPositions", $"weight.{InputKind.SignalRsi}"],
        };

        var space = _engine.CreateParameterSpace(seed);

        // Act
        var built = space
            .Build([.. space.Parameters.Select(p => p.Min)])
            .ShouldBeOfType<SwingConfiguration>();

        // Assert
        built.MaxPositions.ShouldBe(4);
        built.InputWeights.ShouldBe([
            new InputWeight(InputKind.SignalTaxAdjustedRoi, -3m),
            new InputWeight(InputKind.SignalRsi, -0.5m),
        ]);
        built.LockedParameters.ShouldBe(["maxPositions", "weight.SignalRsi"]);
    }
}
