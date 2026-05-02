using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly RunBacktestConsumer _consumer;
    private readonly ILogger<RunBacktestConsumer> _logger = Substitute.For<ILogger<RunBacktestConsumer>>();

    public RunBacktestConsumerTests()
    {
        _fixture.Customize(new IdCustomization());
        var dbContext = DbContextExtensions.Mock<PlutusDbContext>();

        var engineLogger = Substitute.For<ILogger<BacktestEngine>>();
        var executors = Substitute.For<IEnumerable<IStrategyExecutor>>();
        var compositeExecutor = new CompositeExecutor([]);
        var signalComputers = Substitute.For<IEnumerable<ISignalComputer>>();
        var engine = new BacktestEngine(engineLogger, dbContext, executors, compositeExecutor, signalComputers);

        _consumer = new RunBacktestConsumer(_logger, dbContext, engine);
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldNotThrow()
    {
        // Arrange
        var message = new RunBacktestMessage(_fixture.Create<Id<Backtest>>());

        // Act
        var run = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await run.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var message = new RunBacktestMessage(_fixture.Create<Id<Backtest>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var run = async () => await _consumer.Handle(message, cancellationToken);

        // Assert
        await run.ShouldThrowAsync<OperationCanceledException>();
    }
}