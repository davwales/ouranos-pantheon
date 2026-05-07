using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class BacktestDataTests
{
    private readonly IFixture _fixture = new Fixture();

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
        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, dateOnly, 100m, 100m, 100m, 500m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);

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
        var dailyAggregates = new List<DailyTradeAggregate> { new(symbolId, otherDate, 100m, 100m, 100m, 500m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);

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
        var dailyAggregates = new List<DailyTradeAggregate> { new(otherSymbolId, dateOnly, 100m, 100m, 100m, 500m) };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);

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
        var shortSnap = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneHour,
            1000m,
            90m,
            110m,
            1000m,
            10,
            1000m,
            1m
        );
        var mediumSnap = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneWeek,
            5000m,
            80m,
            120m,
            5000m,
            50,
            1000m,
            5m
        );
        var longSnap = MarketTradeSnapshot.Create(
            marketId,
            symbolId,
            TimeFrame.OneMonth,
            20000m,
            70m,
            130m,
            20000m,
            200,
            1000m,
            20m
        );
        var allSnapshots = new List<MarketTradeSnapshot> { shortSnap, mediumSnap, longSnap };

        var data = BacktestData.FromRaw(
            market,
            [],
            allSnapshots,
            [],
            [],
            []
        );

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(symbolId, DateTimeOffset.MaxValue);

        // Assert
        shortResult.ShouldNotBeNull();
        mediumResult.ShouldNotBeNull();
        longResult.ShouldNotBeNull();
        shortResult.TimeFrame.ShouldBe(TimeFrame.OneHour);
        mediumResult.TimeFrame.ShouldBe(TimeFrame.OneWeek);
        longResult.TimeFrame.ShouldBe(TimeFrame.OneMonth);
    }

    [Fact]
    public void GetSnapshotsForSymbol_WhenNoSnapshotsForSymbol_ReturnsAllNull()
    {
        // Arrange
        var targetSymbolId = _fixture.Create<Id<Symbol>>();
        var otherSymbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var snap = MarketTradeSnapshot.Create(
            marketId,
            otherSymbolId,
            TimeFrame.OneHour,
            1000m,
            90m,
            110m,
            1000m,
            10,
            1000m,
            1m
        );
        var allSnapshots = new List<MarketTradeSnapshot> { snap };

        var data = BacktestData.FromRaw(
            market,
            [],
            allSnapshots,
            [],
            [],
            []
        );

        // Act
        var (shortResult, mediumResult, longResult) =
            data.GetSnapshotsForSymbol(targetSymbolId, DateTimeOffset.MaxValue);

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
        var allSnapshots = new List<MarketTradeSnapshot>();

        var data = BacktestData.FromRaw(
            market,
            [],
            allSnapshots,
            [],
            [],
            []
        );

        // Act
        var (shortResult, mediumResult, longResult) = data.GetSnapshotsForSymbol(symbolId, DateTimeOffset.MaxValue);

        // Assert
        shortResult.ShouldBeNull();
        mediumResult.ShouldBeNull();
        longResult.ShouldBeNull();
    }
}
