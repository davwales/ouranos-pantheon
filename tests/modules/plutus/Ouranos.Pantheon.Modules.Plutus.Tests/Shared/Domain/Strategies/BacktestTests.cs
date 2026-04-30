using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies;

public sealed class BacktestTests
{
    private readonly IFixture _fixture = new Fixture();

    public BacktestTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenValidInput_ShouldSetPendingStatus()
    {
        // Arrange
        var strategyId = _fixture.Create<Id<Strategy>>();
        var marketId = _fixture.Create<Id<Market>>();
        var startDate = DateTimeOffset.UtcNow;
        var endDate = startDate.AddDays(30);
        var budget = 10000m;

        // Act
        var backtest = Backtest.Create(strategyId, marketId, startDate, endDate, budget);

        // Assert
        backtest.Id.ShouldNotBe(default);
        backtest.StrategyId.ShouldBe(strategyId);
        backtest.MarketId.ShouldBe(marketId);
        backtest.StartDate.ShouldBe(startDate);
        backtest.EndDate.ShouldBe(endDate);
        backtest.Budget.ShouldBe(budget);
        backtest.Status.ShouldBe(BacktestStatus.Pending);
        backtest.Results.ShouldBeNull();
        backtest.ErrorMessage.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenBudgetIsInvalid_ShouldThrowArgumentException(decimal budget)
    {
        // Arrange
        var strategyId = _fixture.Create<Id<Strategy>>();
        var marketId = _fixture.Create<Id<Market>>();
        var startDate = DateTimeOffset.UtcNow;
        var endDate = startDate.AddDays(30);

        // Act
        var create = () => Backtest.Create(strategyId, marketId, startDate, endDate, budget);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenEndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var strategyId = _fixture.Create<Id<Strategy>>();
        var marketId = _fixture.Create<Id<Market>>();
        var startDate = DateTimeOffset.UtcNow;
        var endDate = startDate.AddDays(-1);

        // Act
        var create = () => Backtest.Create(strategyId, marketId, startDate, endDate, 1000m);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenEndDateEqualsStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var strategyId = _fixture.Create<Id<Strategy>>();
        var marketId = _fixture.Create<Id<Market>>();
        var date = DateTimeOffset.UtcNow;

        // Act
        var create = () => Backtest.Create(strategyId, marketId, date, date, 1000m);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void MarkRunning_ShouldSetRunningStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        // Act
        backtest.MarkRunning();

        // Assert
        backtest.Status.ShouldBe(BacktestStatus.Running);
    }

    [Fact]
    public void Complete_WhenValidResults_ShouldSetCompletedStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        var results = new BacktestResults(
            TotalReturn: 100m,
            TotalReturnPercent: 0.1m,
            MaxDrawdown: 50m,
            MaxDrawdownPercent: 0.05m,
            WinRate: 0.6m,
            TotalTrades: 10,
            WinningTrades: 6,
            LosingTrades: 4,
            SharpeRatio: 1.5m,
            AverageTradeReturn: 10m,
            BestTrade: 50m,
            WorstTrade: -20m,
            FinalBalance: 11000m,
            Positions: []
        );

        // Act
        backtest.Complete(results);

        // Assert
        backtest.Status.ShouldBe(BacktestStatus.Completed);
        backtest.Results.ShouldBe(results);
    }

    [Fact]
    public void Complete_WhenNullResults_ShouldThrowArgumentNullException()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        // Act
        var complete = () => backtest.Complete(null!);

        // Assert
        complete.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Fail_WhenValidMessage_ShouldSetFailedStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        // Act
        backtest.Fail("Something went wrong");

        // Assert
        backtest.Status.ShouldBe(BacktestStatus.Failed);
        backtest.ErrorMessage.ShouldBe("Something went wrong");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Fail_WhenMessageIsInvalid_ShouldThrowArgumentException(string? message)
    {
        // Arrange
        var backtest = CreateValidBacktest();

        // Act
        var fail = () => backtest.Fail(message!);

        // Assert
        fail.ShouldThrow<ArgumentException>();
    }

    private Backtest CreateValidBacktest()
    {
        return Backtest.Create(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
    }
}