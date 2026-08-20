using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using Wolverine;
using Strategy = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Strategy;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RestartBacktest;

public sealed class RestartBacktestHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly RestartBacktestHandler _handler;
    private readonly ILogger<RestartBacktestHandler> _logger = Substitute.For<
        ILogger<RestartBacktestHandler>
    >();
    private readonly PlutusDbContext _dbContext;
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    public RestartBacktestHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new RestartBacktestHandler(_logger, _dbContext, _bus);
    }

    [Fact]
    public async Task Handle_WhenFailedBacktest_ShouldRestartSuccessfully()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
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
        backtest.Fail("Something went wrong");

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var command = new RestartBacktestInput(backtest.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<RestartBacktestResponse>();
        result.Status.ShouldBe(BacktestStatus.Pending);
        result.BacktestId.ShouldBe(backtest.Id);

        var saved = await _dbContext.Backtests.FindAsync(backtest.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(BacktestStatus.Pending);
        saved.ErrorMessage.ShouldBeNull();
        saved.ProgressPercent.ShouldBe(0);
        saved.Results.ShouldBeNull();

        await _bus.Received(1)
            .PublishAsync(Arg.Is<RunBacktestMessage>(m => m.BacktestId == backtest.Id));
    }

    [Fact]
    public async Task Handle_WhenCancelledBacktest_ShouldRestartSuccessfully()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
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
        backtest.Cancel("Cancelled by user");

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var command = new RestartBacktestInput(backtest.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(BacktestStatus.Pending);
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldThrow()
    {
        // Arrange
        var command = new RestartBacktestInput(_fixture.Create<Id<Backtest>>());

        // Act
        var restart = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await restart.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenPendingBacktest_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
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

        var command = new RestartBacktestInput(backtest.Id);

        // Act
        var restart = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await restart.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new RestartBacktestInput(_fixture.Create<Id<Backtest>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var restart = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await restart.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenFailedBacktestWithResults_ShouldClearResults()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
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
        backtest.Fail("Simulated failure");

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var command = new RestartBacktestInput(backtest.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<RestartBacktestResponse>();
        result.Status.ShouldBe(BacktestStatus.Pending);

        var saved = await _dbContext.Backtests.FindAsync(backtest.Id);
        saved.ShouldNotBeNull();
        saved.Status.ShouldBe(BacktestStatus.Pending);
        saved.Results.ShouldBeNull();
    }
}
