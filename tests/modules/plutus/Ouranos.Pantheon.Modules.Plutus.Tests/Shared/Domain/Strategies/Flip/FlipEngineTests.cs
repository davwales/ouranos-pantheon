using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Flip;

public sealed class FlipEngineTests
{
    private static readonly Id<Market> MarketId = new(Guid.NewGuid().ToString());

    private readonly IFlipDataQueryService _dataService = Substitute.For<IFlipDataQueryService>();
    private readonly IStrategyEngine _engine;

    public FlipEngineTests()
    {
        _engine = new FlipEngine(_dataService);
    }

    [Fact]
    public void Constructor_WhenDataServiceIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new FlipEngine(null!));
    }

    [Fact]
    public void Mode_ShouldBeFlip()
    {
        // Act
        var mode = _engine.Mode;

        // Assert
        mode.ShouldBe(StrategyMode.Flip);
    }

    [Theory]
    [InlineData(24, 168)]
    [InlineData(200, 200)]
    public void WarmUpStart_ShouldCoverTheLongerOfLookbackAndPriceMoveWindow(
        int lookbackHours,
        int expectedHours
    )
    {
        // Arrange
        var configuration = new FlipConfiguration(LookbackHours: lookbackHours);

        // Act
        var start = FlipEngine.WarmUpStart(FlipTestData.Start, configuration);

        // Assert
        start.ShouldBe(FlipTestData.Start.AddHours(-expectedHours));
    }

    [Fact]
    public void LastCompleteHour_ShouldUseTheNewestTradedBarAcrossAllSeries()
    {
        // Arrange
        var early = FlipTestData.Series(
            5,
            i => i <= 13 ? new FlipBar(120f, 10f, 100f, 10f) : default
        );

        var late = FlipTestData.Series(5, i => i == 30 ? new FlipBar(0f, 0f, 100f, 10f) : default);

        // Act
        var hour = FlipEngine.LastCompleteHour(FlipTestData.Market(early, late));

        // Assert
        hour.ShouldBe(2);
    }

    [Fact]
    public void LastCompleteHour_WhenNothingTraded_ShouldBeZero()
    {
        // Arrange
        var series = FlipTestData.Series(5, _ => default);

        // Act
        var hour = FlipEngine.LastCompleteHour(FlipTestData.Market(series));

        // Assert
        hour.ShouldBe(0);
    }

    [Fact]
    public void CreateParameterSpace_ShouldTuneQuotingAndSizingParameters()
    {
        // Act
        var space = _engine.CreateParameterSpace(new FlipConfiguration());

        // Assert
        space
            .Parameters.Select(p => p.Key)
            .ShouldBe([
                "lookbackHours",
                "bidOffset",
                "askOffset",
                "minMargin",
                "maxHoldHours",
                "maxPositionPercent",
                "maxPriceMove",
            ]);

        space.Seed.ShouldBe([24d, 0d, 0d, 0.03d, 72d, 0.125d, 0.1d]);
    }

    [Fact]
    public void CreateParameterSpace_ShouldClampTheSeedIntoRange()
    {
        // Arrange
        var seed = new FlipConfiguration(
            LookbackHours: 1,
            BidOffset: 1m,
            MaxHoldHours: 1000,
            MaxPositionPercent: 1m
        );

        // Act
        var space = _engine.CreateParameterSpace(seed);

        // Assert
        space.Seed[0].ShouldBe(4d);
        space.Seed[1].ShouldBe(0.4d);
        space.Seed[4].ShouldBe(168d);
        space.Seed[5].ShouldBe(0.5d);
    }

    [Fact]
    public void CreateParameterSpace_WhenBuilding_ShouldKeepTheTradersOwnConstraintsFromTheSeed()
    {
        // Arrange
        var seed = new FlipConfiguration(CheckInHours: 4, MaxOpenOrders: 3, MinPrice: 100m);
        var space = _engine.CreateParameterSpace(seed);

        // Act
        var built = space
            .Build([48d, 0.1d, -0.2d, 0.05d, 24d, 0.25d, 0.2d])
            .ShouldBeOfType<FlipConfiguration>();

        // Assert
        built.ShouldBe(
            new FlipConfiguration(
                CheckInHours: 4,
                MaxOpenOrders: 3,
                LookbackHours: 48,
                BidOffset: 0.1m,
                AskOffset: -0.2m,
                MinMargin: 0.05m,
                MaxHoldHours: 24,
                MaxPositionPercent: 0.25m,
                MaxPriceMove: 0.2m,
                MinPrice: 100m
            )
        );
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadFromTheWarmUpStart()
    {
        // Arrange
        var configuration = new FlipConfiguration(LookbackHours: 24);
        var end = FlipTestData.Start.AddDays(2);
        var data = FlipTestData.Market(FlipTestData.Bouncing(10, 100f, 120f));

        _dataService
            .LoadAsync(MarketId, Arg.Any<DateTimeOffset>(), end, Arg.Any<CancellationToken>())
            .Returns(data);

        // Act
        var loaded = await _engine.LoadAsync(
            MarketId,
            FlipTestData.Start,
            end,
            configuration,
            CancellationToken.None
        );

        // Assert
        loaded.ShouldBeSameAs(data);

        await _dataService
            .Received(1)
            .LoadAsync(
                MarketId,
                FlipTestData.Start.AddHours(-168),
                end,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public void Simulate_WhenConfigurationIsSwing_ShouldThrowArgumentException()
    {
        // Arrange
        var data = FlipTestData.Market(FlipTestData.Bouncing(10, 100f, 120f));

        var settings = new SimulationSettings(
            FlipTestData.Start,
            FlipTestData.Start.AddHours(8),
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
    public async Task RecommendAsync_ShouldRankBySpreadAndSizeOffersLikeTheSimulator()
    {
        // Arrange
        ArrangeData(
            FlipTestData.Bouncing(200, 100f, 110f, name: "Narrow"),
            FlipTestData.Bouncing(200, 100f, 130f, name: "Wide"),
            FlipTestData.Bouncing(200, 100f, 120f, name: "Medium")
        );

        var configuration = new FlipConfiguration(MaxPositionPercent: 0.25m);

        // Act
        var recommendations = await _engine.RecommendAsync(
            MarketId,
            configuration,
            10_000m,
            BacktestDataOptions.DefaultVolumeParticipationRate,
            CancellationToken.None
        );

        // Assert
        recommendations.Select(r => r.SymbolName).ShouldBe(["Wide", "Medium", "Narrow"]);

        var wide = recommendations[0];
        wide.BuyPrice.ShouldBe(100m);
        wide.SellPrice.ShouldBe(130m);
        wide.SuggestedVolume.ShouldBe(25m);
        wide.SuggestedAllocation.ShouldBe(2500m);
        wide.NetMargin.ShouldBe(0.28m);
        wide.ExpectedProfit.ShouldBe(700m);
        wide.LimitedBy.ShouldBe(PositionLimit.Budget);
    }

    [Fact]
    public async Task RecommendAsync_WithALowerParticipationRate_ShouldSuggestFewerUnits()
    {
        // Arrange
        ArrangeData(FlipTestData.Bouncing(200, 100f, 130f, name: "Wide"));
        var configuration = new FlipConfiguration(MaxPositionPercent: 1m);

        // Act
        var full = await _engine.RecommendAsync(
            MarketId,
            configuration,
            1_000_000m,
            1m,
            CancellationToken.None
        );
        var capped = await _engine.RecommendAsync(
            MarketId,
            configuration,
            1_000_000m,
            0.01m,
            CancellationToken.None
        );

        // Assert
        var cappedVolume = capped.ShouldHaveSingleItem().SuggestedVolume;
        cappedVolume.ShouldBeLessThan(full.ShouldHaveSingleItem().SuggestedVolume);
        capped[0].LimitedBy.ShouldBe(PositionLimit.Liquidity);
    }

    [Fact]
    public async Task RecommendAsync_ShouldNotExceedTheBudget()
    {
        // Arrange
        ArrangeData(
            FlipTestData.Bouncing(200, 100f, 130f, name: "A"),
            FlipTestData.Bouncing(200, 100f, 125f, name: "B"),
            FlipTestData.Bouncing(200, 100f, 120f, name: "C")
        );

        var configuration = new FlipConfiguration(MaxPositionPercent: 0.5m);

        // Act
        var recommendations = await _engine.RecommendAsync(
            MarketId,
            configuration,
            10_000m,
            BacktestDataOptions.DefaultVolumeParticipationRate,
            CancellationToken.None
        );

        // Assert
        recommendations.Count.ShouldBe(2);
        recommendations.ShouldAllBe(r => r.SuggestedAllocation <= 5000m);
        recommendations.Sum(r => r.SuggestedAllocation).ShouldBeLessThanOrEqualTo(10_000m);
    }

    [Fact]
    public async Task RecommendAsync_ShouldNotExceedMaxOpenOrders()
    {
        // Arrange
        ArrangeData([
            .. Enumerable
                .Range(0, 5)
                .Select(i => FlipTestData.Bouncing(200, 100f, 120f + i, name: $"Item{i}")),
        ]);

        var configuration = new FlipConfiguration(MaxOpenOrders: 3, MaxPositionPercent: 0.1m);

        // Act
        var recommendations = await _engine.RecommendAsync(
            MarketId,
            configuration,
            10_000m,
            BacktestDataOptions.DefaultVolumeParticipationRate,
            CancellationToken.None
        );

        // Assert
        recommendations.Count.ShouldBe(3);
    }

    [Fact]
    public async Task RecommendAsync_WhenTheBuyLimitBinds_ShouldBeLimitedByBuyLimit()
    {
        // Arrange
        ArrangeData(FlipTestData.Bouncing(200, 100f, 130f, buyLimit: 10m));

        // Act
        var recommendations = await _engine.RecommendAsync(
            MarketId,
            new FlipConfiguration(),
            1_000_000m,
            BacktestDataOptions.DefaultVolumeParticipationRate,
            CancellationToken.None
        );

        // Assert
        var recommendation = recommendations.ShouldHaveSingleItem();
        recommendation.SuggestedVolume.ShouldBe(20m);
        recommendation.LimitedBy.ShouldBe(PositionLimit.BuyLimit);
    }

    [Fact]
    public async Task RecommendAsync_WhenNoItemClearsTheMinimumMargin_ShouldReturnNothing()
    {
        // Arrange
        ArrangeData(FlipTestData.Bouncing(200, 100f, 101f));

        // Act
        var recommendations = await _engine.RecommendAsync(
            MarketId,
            new FlipConfiguration(),
            10_000m,
            BacktestDataOptions.DefaultVolumeParticipationRate,
            CancellationToken.None
        );

        // Assert
        recommendations.ShouldBeEmpty();
    }

    private void ArrangeData(params FlipSeries[] series)
    {
        _dataService
            .LoadAsync(
                MarketId,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(FlipTestData.Market(series));
    }

    [Fact]
    public void CreateParameterSpace_WhenParametersAreLocked_ShouldKeepThemAtTheSeedValue()
    {
        // Arrange
        var seed = new FlipConfiguration(BidOffset: 0.6m, MaxHoldHours: 24) with
        {
            LockedParameters = ["bidOffset", "maxHoldHours"],
        };

        var space = _engine.CreateParameterSpace(seed);

        // Act
        var built = space
            .Build([.. space.Parameters.Select(p => p.Max)])
            .ShouldBeOfType<FlipConfiguration>();

        // Assert
        built.BidOffset.ShouldBe(0.6m);
        built.MaxHoldHours.ShouldBe(24);
        built.AskOffset.ShouldBe(0.4m);
        built.LockedParameters.ShouldBe(["bidOffset", "maxHoldHours"]);
    }

    [Fact]
    public void CreateParameterSpace_WhenLockingANeverOptimizedField_ShouldThrowArgumentException()
    {
        // Arrange
        var seed = new FlipConfiguration() with
        {
            LockedParameters = ["minPrice"],
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => _engine.CreateParameterSpace(seed));
    }
}
