using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class TrackMetricsStepTests
{
    private readonly IFixture _fixture = new Fixture();

    public TrackMetricsStepTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoOpenPositions_RecordsBalanceAsPortfolioValue()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);
        payload.Portfolio.Balance = 8000m;

        var context = new PipelineContext(CancellationToken.None);
        var step = new TrackMetricsStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.PortfolioValues.ShouldContain(8000m);
        payload.Portfolio.PeakPortfolioValue.ShouldBe(10000m); // initial value was 10000
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceIncreases_UpdatesPeak()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);
        payload.Portfolio.Balance = 12000m;
        payload.Portfolio.PeakPortfolioValue = 10000m;

        var context = new PipelineContext(CancellationToken.None);
        var step = new TrackMetricsStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.PeakPortfolioValue.ShouldBe(12000m);
        payload.Portfolio.MaxDrawdown.ShouldBe(0m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceDecreases_UpdatesDrawdown()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m
        );
        var payload = new BacktestPayload(parameters);
        var data = BacktestData.FromRaw(market, [], [], [], [], []);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);
        payload.Portfolio.Balance = 8000m;
        payload.Portfolio.PeakPortfolioValue = 10000m;

        var context = new PipelineContext(CancellationToken.None);
        var step = new TrackMetricsStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.PeakPortfolioValue.ShouldBe(10000m);
        payload.Portfolio.MaxDrawdown.ShouldBe(0.2m); // (10000 - 8000) / 10000 = 0.2
    }

    [Fact]
    public async Task ExecuteAsync_WhenOpenPositionsExist_IncludesPositionValue()
    {
        // Arrange
        var symbolId = _fixture.Create<Id<Symbol>>();
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        var executor = Substitute.For<IStrategyExecutor>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var startDate = DateTimeOffset.UtcNow.AddDays(-10);
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            DateTimeOffset.UtcNow,
            10000m
        );

        var payload = new BacktestPayload(parameters);
        var currentDate = startDate.AddDays(5);
        var dateOnly = DateOnly.FromDateTime(currentDate.UtcDateTime);
        var dailyAggregates = new List<DailyTradeAggregate>
        {
            new(symbolId, dateOnly, 200m, 200m, 200m, 1000m),
        };
        var data = BacktestData.FromRaw(market, [], [], [], [], dailyAggregates);
        payload.Context = new BacktestContext(data, executor, 0m, 7, startDate);
        payload.Portfolio.Balance = 5000m;
        payload.Portfolio.OpenPositions[symbolId] = new OpenPosition(
            symbolId,
            "SYM",
            null,
            100m,
            10m,
            currentDate
        );

        var context = new PipelineContext(CancellationToken.None) { CurrentIteration = 5 };
        var step = new TrackMetricsStep();

        // Act
        await step.ExecuteAsync(context, payload);

        // Assert
        payload.Portfolio.PortfolioValues.ShouldContain(7000m);
    }
}
