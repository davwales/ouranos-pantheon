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
        backtest.StartDate.ShouldBe(startDate.ToUniversalTime());
        backtest.EndDate.ShouldBe(endDate.ToUniversalTime());
        backtest.Budget.ShouldBe(budget);
        backtest.Status.ShouldBe(BacktestStatus.Pending);
        backtest.Results.ShouldBeNull();
        backtest.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void Create_WhenNonUtcOffset_ShouldNormalizeToUtc()
    {
        // Arrange
        var strategyId = _fixture.Create<Id<Strategy>>();
        var marketId = _fixture.Create<Id<Market>>();
        var localOffset = TimeSpan.FromHours(2);
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, localOffset);
        var endDate = startDate.AddDays(30);
        var budget = 10000m;

        // Act
        var backtest = Backtest.Create(strategyId, marketId, startDate, endDate, budget);

        // Assert
        backtest.StartDate.Offset.ShouldBe(TimeSpan.Zero);
        backtest.EndDate.Offset.ShouldBe(TimeSpan.Zero);
        backtest.StartDate.UtcDateTime.ShouldBe(startDate.UtcDateTime);
        backtest.EndDate.UtcDateTime.ShouldBe(endDate.UtcDateTime);
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
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBe("Loading market data...");
    }

    [Fact]
    public void Complete_WhenValidResults_ShouldSetCompletedStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        var results = new BacktestResults
        {
            TotalReturn = 100m,
            TotalReturnPercent = 0.1m,
            MaxDrawdown = 50m,
            MaxDrawdownPercent = 0.05m,
            WinRate = 0.6m,
            TotalTrades = 10,
            WinningTrades = 6,
            LosingTrades = 4,
            SharpeRatio = 1.5m,
            SortinoRatio = 2.0m,
            CalmarRatio = 1.0m,
            Cagr = 0.08m,
            ProfitFactor = 1.5m,
            Expectancy = 5m,
            AverageTradeReturn = 0.1m,
            BestTrade = 0.5m,
            WorstTrade = -0.2m,
            FinalBalance = 11000m,
        };

        // Act
        backtest.Complete(results);

        // Assert
        backtest.Status.ShouldBe(BacktestStatus.Completed);
        backtest.Results.ShouldNotBeNull();
        backtest.Results.TotalReturn.ShouldBe(results.TotalReturn);
        backtest.Results.TotalReturnPercent.ShouldBe(results.TotalReturnPercent);
        backtest.Results.MaxDrawdown.ShouldBe(results.MaxDrawdown);
        backtest.Results.MaxDrawdownPercent.ShouldBe(results.MaxDrawdownPercent);
        backtest.Results.WinRate.ShouldBe(results.WinRate);
        backtest.Results.TotalTrades.ShouldBe(results.TotalTrades);
        backtest.Results.WinningTrades.ShouldBe(results.WinningTrades);
        backtest.Results.LosingTrades.ShouldBe(results.LosingTrades);
        backtest.Results.SharpeRatio.ShouldBe(results.SharpeRatio);
        backtest.Results.SortinoRatio.ShouldBe(results.SortinoRatio);
        backtest.Results.CalmarRatio.ShouldBe(results.CalmarRatio);
        backtest.Results.Cagr.ShouldBe(results.Cagr);
        backtest.Results.ProfitFactor.ShouldBe(results.ProfitFactor);
        backtest.Results.Expectancy.ShouldBe(results.Expectancy);
        backtest.Results.AverageTradeReturn.ShouldBe(results.AverageTradeReturn);
        backtest.Results.BestTrade.ShouldBe(results.BestTrade);
        backtest.Results.WorstTrade.ShouldBe(results.WorstTrade);
        backtest.Results.FinalBalance.ShouldBe(results.FinalBalance);
        backtest.Positions.ShouldBeEmpty();
        backtest.Results.OptimizedConfiguration.ShouldBe(results.OptimizedConfiguration);
    }

    [Fact]
    public void Complete_WhenNullResults_ShouldThrowArgumentNullException()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        var complete = () =>
        {
            backtest.Complete(null!);
        };

        // Assert
        complete.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Complete_WhenNotRunning_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        var results = new BacktestResults
        {
            TotalReturn = 100m,
            TotalReturnPercent = 0.1m,
            MaxDrawdown = 50m,
            MaxDrawdownPercent = 0.05m,
            WinRate = 0.6m,
            TotalTrades = 10,
            WinningTrades = 6,
            LosingTrades = 4,
            SharpeRatio = 1.5m,
            SortinoRatio = 2.0m,
            CalmarRatio = 1.0m,
            Cagr = 0.08m,
            ProfitFactor = 1.5m,
            Expectancy = 5m,
            AverageTradeReturn = 0.1m,
            BestTrade = 0.5m,
            WorstTrade = -0.2m,
            FinalBalance = 11000m,
        };

        // Act
        var complete = () =>
        {
            backtest.Complete(results);
        };

        // Assert
        complete.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Fail_WhenValidMessageFromRunningState_ShouldSetFailedStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        backtest.Fail("Something went wrong");

        // Assert
        backtest.Status.ShouldBe(BacktestStatus.Failed);
        backtest.ErrorMessage.ShouldBe("Something went wrong");
    }

    [Fact]
    public void Fail_WhenValidMessageFromPendingState_ShouldSetFailedStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        // Act
        backtest.Fail("Something went wrong");

        // Assert
        backtest.Status.ShouldBe(BacktestStatus.Failed);
        backtest.ErrorMessage.ShouldBe("Something went wrong");
    }

    [Fact]
    public void Fail_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();
        backtest.Complete(new BacktestResults());

        // Act
        var fail = () =>
        {
            backtest.Fail("Something went wrong");
        };

        // Assert
        fail.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void MarkRunning_WhenAlreadyRunning_ShouldReturnFalse()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        var result = backtest.MarkRunning();

        // Assert
        result.ShouldBeFalse();
        backtest.Status.ShouldBe(BacktestStatus.Running);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFalse()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();
        backtest.Complete(new BacktestResults());

        // Act
        var result = backtest.Complete(new BacktestResults());

        // Assert
        result.ShouldBeFalse();
        backtest.Status.ShouldBe(BacktestStatus.Completed);
    }

    [Fact]
    public void Fail_WhenAlreadyFailed_ShouldReturnFalse()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.Fail("Something went wrong");

        // Act
        var result = backtest.Fail("Another error");

        // Assert
        result.ShouldBeFalse();
        backtest.Status.ShouldBe(BacktestStatus.Failed);
        backtest.ErrorMessage.ShouldBe("Something went wrong");
    }

    [Fact]
    public void MarkRunning_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();
        backtest.Complete(new BacktestResults());

        // Act
        var markRunning = () =>
        {
            backtest.MarkRunning();
        };

        // Assert
        markRunning.ShouldThrow<InvalidOperationException>();
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
        var fail = () =>
        {
            backtest.Fail(message!);
        };

        // Assert
        fail.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void UpdateProgress_WhenRunning_ShouldUpdatePercentAndMessage()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        backtest.UpdateProgress(50, "Simulating day 25 of 50...");

        // Assert
        backtest.ProgressPercent.ShouldBe(50);
        backtest.ProgressMessage.ShouldBe("Simulating day 25 of 50...");
    }

    [Fact]
    public void UpdateProgress_WhenPercentAbove100_ShouldClampTo100()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        backtest.UpdateProgress(150, "Over 100%");

        // Assert
        backtest.ProgressPercent.ShouldBe(100);
    }

    [Fact]
    public void UpdateProgress_WhenPercentBelow0_ShouldClampTo0()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        backtest.UpdateProgress(-10, "Under 0%");

        // Assert
        backtest.ProgressPercent.ShouldBe(0);
    }

    [Fact]
    public void UpdateProgress_WhenNotRunning_ShouldDoNothing()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        // Act
        var update = () => backtest.UpdateProgress(50, "Should not work");

        // Assert
        update.ShouldNotThrow();
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateProgress_WhenMessageIsInvalid_ShouldThrowArgumentException(string? message)
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        var update = () => backtest.UpdateProgress(50, message!);

        // Assert
        update.ShouldThrow<ArgumentException>();
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

    [Fact]
    public void Cancel_WhenPending_ShouldSetCancelledStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();

        // Act
        var result = backtest.Cancel();

        // Assert
        result.ShouldBeTrue();
        backtest.Status.ShouldBe(BacktestStatus.Cancelled);
        backtest.ErrorMessage.ShouldBe("Cancelled by user.");
    }

    [Fact]
    public void Cancel_WhenRunning_ShouldSetCancelledStatus()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        var result = backtest.Cancel();

        // Assert
        result.ShouldBeTrue();
        backtest.Status.ShouldBe(BacktestStatus.Cancelled);
        backtest.ErrorMessage.ShouldBe("Cancelled by user.");
    }

    [Fact]
    public void Cancel_WhenCustomReason_ShouldSetCustomErrorMessage()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        // Act
        backtest.Cancel("Server shutdown");

        // Assert
        backtest.ErrorMessage.ShouldBe("Server shutdown");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldReturnFalse()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.Cancel();

        // Act
        var result = backtest.Cancel();

        // Assert
        result.ShouldBeFalse();
        backtest.Status.ShouldBe(BacktestStatus.Cancelled);
    }

    [Theory]
    [InlineData(BacktestStatus.Completed)]
    [InlineData(BacktestStatus.Failed)]
    public void Cancel_WhenInvalidState_ShouldThrowInvalidOperationException(
        BacktestStatus targetStatus
    )
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();

        if (targetStatus == BacktestStatus.Completed)
        {
            backtest.Complete(new BacktestResults());
        }
        else
        {
            backtest.Fail("error");
        }

        // Act
        var cancel = () =>
        {
            backtest.Cancel();
        };

        // Assert
        cancel.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Restart_WhenFailed_ShouldResetToPending()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();
        backtest.Fail("Something went wrong");

        // Act
        var result = backtest.Restart();

        // Assert
        result.ShouldBeTrue();
        backtest.Status.ShouldBe(BacktestStatus.Pending);
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBeNull();
        backtest.Results.ShouldBeNull();
        backtest.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void Restart_WhenCancelled_ShouldResetToPending()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();
        backtest.Cancel("Cancelled by user");

        // Act
        var result = backtest.Restart();

        // Assert
        result.ShouldBeTrue();
        backtest.Status.ShouldBe(BacktestStatus.Pending);
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBeNull();
        backtest.Results.ShouldBeNull();
        backtest.ErrorMessage.ShouldBeNull();
    }

    [Theory]
    [InlineData(BacktestStatus.Pending)]
    [InlineData(BacktestStatus.Running)]
    [InlineData(BacktestStatus.Completed)]
    public void Restart_WhenInvalidState_ShouldThrowInvalidOperationException(
        BacktestStatus targetStatus
    )
    {
        // Arrange
        var backtest = CreateValidBacktest();

        switch (targetStatus)
        {
            case BacktestStatus.Running:
                backtest.MarkRunning();
                break;
            case BacktestStatus.Completed:
                backtest.MarkRunning();
                backtest.Complete(new BacktestResults());
                break;
        }

        // Act
        var restart = () =>
        {
            backtest.Restart();
        };

        // Assert
        restart.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_ThenRestart_ThenMarkRunning_ShouldTransitionCorrectly()
    {
        // Arrange
        var backtest = CreateValidBacktest();
        backtest.MarkRunning();
        backtest.Cancel("Server shutdown");
        backtest.Restart();

        // Act
        backtest.MarkRunning();

        // Assert
        backtest.Status.ShouldBe(BacktestStatus.Running);
        backtest.ProgressPercent.ShouldBe(0);
        backtest.ProgressMessage.ShouldBe("Loading market data...");
    }
}
