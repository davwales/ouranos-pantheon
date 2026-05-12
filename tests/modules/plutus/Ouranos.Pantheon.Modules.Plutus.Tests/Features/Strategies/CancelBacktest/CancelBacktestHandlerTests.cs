using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using Strategy = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Strategy;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.CancelBacktest;

public sealed class CancelBacktestHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CancelBacktestHandler _handler;
    private readonly ILogger<CancelBacktestHandler> _logger = Substitute.For<
        ILogger<CancelBacktestHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public CancelBacktestHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new CancelBacktestHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenPendingBacktest_ShouldCancelSuccessfully()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            strategy
        );

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var command = new CancelBacktestInput(backtest.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CancelBacktestResponse>();
        result.Status.ShouldBe(BacktestStatus.Cancelled);
        result.BacktestId.ShouldBe(backtest.Id);

        var saved = await _dbContext.Backtests.FindAsync(backtest.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(BacktestStatus.Cancelled);
        saved.ErrorMessage.ShouldBe("Cancelled by user.");
    }

    [Fact]
    public async Task Handle_WhenRunningBacktest_ShouldCancelSuccessfully()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            strategy
        );
        backtest.MarkRunning();

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var command = new CancelBacktestInput(backtest.Id, "Server shutdown");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(BacktestStatus.Cancelled);

        var saved = await _dbContext.Backtests.FindAsync(backtest.Id);
        saved.ShouldNotBeNull();
        saved.ErrorMessage.ShouldBe("Server shutdown");
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldThrow()
    {
        // Arrange
        var command = new CancelBacktestInput(_fixture.Create<Id<Backtest>>());

        // Act
        var cancel = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await cancel.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCompletedBacktest_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            strategy
        );
        backtest.MarkRunning();
        backtest.Complete(new BacktestResults());

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var command = new CancelBacktestInput(backtest.Id);

        // Act
        var cancel = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await cancel.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CancelBacktestInput(_fixture.Create<Id<Backtest>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var cancel = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await cancel.ShouldThrowAsync<OperationCanceledException>();
    }
}
