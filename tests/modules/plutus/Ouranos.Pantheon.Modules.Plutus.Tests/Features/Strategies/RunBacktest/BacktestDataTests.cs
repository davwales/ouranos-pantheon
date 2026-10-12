using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class BacktestDataTests
{
    private readonly Fixture _fixture = new();

    public BacktestDataTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void GetDailyVolume_WhenAggregateExists_ReturnsTotalVolume()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var currentDate = DateTimeOffset.UtcNow;
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);

        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 100m, 100m, 100m, 500m),
        };

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetDailyVolume(symbolId, currentDate);

        // Assert
        result.ShouldBe(500m);
    }

    [Fact]
    public void GetDailyVolume_WhenNoDataForDate_ReturnsZero()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var currentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var otherDate = new DateOnly(2025, 1, 10);

        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, otherDate, 100m, 100m, 100m, 500m),
        };

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetDailyVolume(symbolId, currentDate);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void GetDailyVolume_WhenNoDataForSymbol_ReturnsZero()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var otherSymbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var currentDate = DateTimeOffset.UtcNow;
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);

        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(otherSymbolId, dateOnly, 100m, 100m, 100m, 500m),
        };

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetDailyVolume(symbolId, currentDate);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void GetSnapshotsForSymbol_WhenSnapshotsExistForAllTimeFrames_ReturnsAllThree()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var asOfDate = DateTimeOffset.UtcNow;
        var dateOnly = DateOnly.FromDateTime(asOfDate.UtcDateTime);

        var dailyAggregates = Enumerable
            .Range(0, 30)
            .Select(i => new DailyTradeAggregate(
                symbolId,
                dateOnly.AddDays(-i),
                100m,
                90m,
                110m,
                1000m
            ))
            .ToList();

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(
            symbolId,
            asOfDate
        );

        // Assert
        shortResult.ShouldNotBeNull();
        mediumResult.ShouldNotBeNull();
        longResult.ShouldNotBeNull();
        shortResult.TimeFrame.ShouldBe(TimeFrame.OneHour);
        mediumResult.TimeFrame.ShouldBe(TimeFrame.OneWeek);
        longResult.TimeFrame.ShouldBe(TimeFrame.OneMonth);

        longResult.TotalVolume.ShouldBe(30 * 1000m);
        shortResult.TotalVolume.ShouldBe(1000m * 60m / 1440m, 0.01m);
    }

    [Fact]
    public void GetSnapshotsForSymbol_WhenNoSnapshotsForSymbol_ReturnsAllNull()
    {
        // Arrange
        var targetSymbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));

        var data = BacktestData.FromRaw(market, [], []);

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(
            targetSymbolId,
            DateTimeOffset.MaxValue
        );

        // Assert
        shortResult.ShouldBeNull();
        mediumResult.ShouldBeNull();
        longResult.ShouldBeNull();
    }

    [Fact]
    public void GetSnapshotsForSymbol_WhenEmptyList_ReturnsAllNull()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));

        var data = BacktestData.FromRaw(market, [], []);

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(
            symbolId,
            DateTimeOffset.MaxValue
        );

        // Assert
        shortResult.ShouldBeNull();
        mediumResult.ShouldBeNull();
        longResult.ShouldBeNull();
    }

    [Fact]
    public void GetWindowAggregates_WithWindowDays_ReturnsRollingWindowOfExactSize()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var endDate = DateTimeOffset.UtcNow;
        var endDateOnly = DateOnly.FromDateTime(endDate.UtcDateTime);

        var dailyAggregates = Enumerable
            .Range(0, 100)
            .Select(i => new DailyTradeAggregate(
                symbolId,
                endDateOnly.AddDays(-i),
                100m,
                90m,
                110m,
                1000m
            ))
            .ToList();

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetWindowAggregates(
            symbolId,
            DateTimeOffset.MinValue,
            endDate,
            windowDays: 30
        );

        // Assert
        result.Count.ShouldBe(30);
        result[0].Date.ShouldBe(endDateOnly.AddDays(-29));
        result[^1].Date.ShouldBe(endDateOnly);
    }

    [Fact]
    public void GetWindowAggregates_WithLegacyMinValueStart_ReturnsAllAggregatesUpToEnd()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var endDate = DateTimeOffset.UtcNow;
        var endDateOnly = DateOnly.FromDateTime(endDate.UtcDateTime);

        var dailyAggregates = Enumerable
            .Range(0, 100)
            .Select(i => new DailyTradeAggregate(
                symbolId,
                endDateOnly.AddDays(-i),
                100m,
                90m,
                110m,
                1000m
            ))
            .ToList();

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetWindowAggregates(symbolId, DateTimeOffset.MinValue, endDate);

        // Assert
        result.Count.ShouldBe(100);
        result[^1].Date.ShouldBe(endDateOnly);
    }

    [Fact]
    public void GetTypicalDailyVolume_WhenSymbolTradesOnFewDays_CountsMissingDaysAsZero()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));
        var currentDate = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        var day = DateOnly.FromDateTime(currentDate.UtcDateTime);

        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, day.AddDays(-5), 100m, 100m, 100m, 1000m),
            new(symbolId, day.AddDays(-2), 100m, 100m, 100m, 1000m),
            new(symbolId, day, 100m, 100m, 100m, 50_000m),
        };

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetTypicalDailyVolume(symbolId, currentDate);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void GetTypicalDailyVolume_ExcludesCurrentDay()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));
        var currentDate = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        var dailyAggregates = BacktestTestData.History(symbolId, currentDate, 100m, 200m);

        dailyAggregates.Add(
            new DailyTradeAggregate(
                symbolId,
                DateOnly.FromDateTime(currentDate.UtcDateTime),
                100m,
                100m,
                100m,
                50_000m
            )
        );

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetTypicalDailyVolume(symbolId, currentDate);

        // Assert
        result.ShouldBe(200m);
    }

    [Theory]
    [InlineData(1000, 200, 50)]
    [InlineData(100, 200, 25)]
    [InlineData(0, 200, 0)]
    public void GetFillCap_UsesSmallerOfTodayAndTypicalVolume(
        int todayVolume,
        int typicalVolume,
        int expectedCap
    )
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));
        var currentDate = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        var dailyAggregates = BacktestTestData.History(symbolId, currentDate, 100m, typicalVolume);

        if (todayVolume > 0)
        {
            dailyAggregates.Add(
                new DailyTradeAggregate(
                    symbolId,
                    DateOnly.FromDateTime(currentDate.UtcDateTime),
                    100m,
                    100m,
                    100m,
                    todayVolume
                )
            );
        }

        var data = BacktestData.FromRaw(market, [], dailyAggregates);

        // Act
        var result = data.GetFillCap(symbolId, currentDate, 0.25m);

        // Assert
        result.ShouldBe(expectedCap);
    }
}
