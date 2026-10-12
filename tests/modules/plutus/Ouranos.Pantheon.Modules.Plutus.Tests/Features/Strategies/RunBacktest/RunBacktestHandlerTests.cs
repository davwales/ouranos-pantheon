using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Wolverine;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class RunBacktestHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly RunBacktestHandler _handler;
    private readonly ILogger<RunBacktestHandler> _logger = Substitute.For<
        ILogger<RunBacktestHandler>
    >();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    public RunBacktestHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
        _handler = new RunBacktestHandler(_logger, _dbContextFactory, _bus);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateBacktestAndPublishMessage()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing());
        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new RunBacktestInput(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            0.2m,
            0.07m
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<RunBacktestResponse>();
        result.BacktestId.ShouldNotBe(default);

        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var backtest = await verifyContext.Backtests.FindAsync(result.BacktestId);
        backtest.ShouldNotBeNull();
        backtest.Status.ShouldBe(BacktestStatus.Pending);
        backtest.StrategyId.ShouldBe(strategy.Id);
        backtest.Kind.ShouldBe(BacktestKind.Backtest);
        backtest.RunSettings.ShouldBe(new BacktestRunSettings(0.2m, 0.07m));
        await _bus.Received(1)
            .PublishAsync(Arg.Is<RunBacktestMessage>(m => m.BacktestId == result.BacktestId));
    }

    [Fact]
    public async Task Handle_WhenRunSettingsOmitted_ShouldPersistDefaults()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing());
        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new RunBacktestInput(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var backtest = await verifyContext.Backtests.FindAsync(result.BacktestId);
        backtest.ShouldNotBeNull();
        backtest.RunSettings.VolumeParticipationRate.ShouldBe(
            BacktestDataOptions.DefaultVolumeParticipationRate
        );
        backtest.RunSettings.SlippageMultiplier.ShouldBe(
            BacktestDataOptions.DefaultSlippageMultiplier
        );
        backtest.RunSettings.Optimization.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var command = new RunBacktestInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        // Act
        var run = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await run.ShouldThrowAsync<Exception>();
        await _bus.DidNotReceiveWithAnyArgs().PublishAsync(default(RunBacktestMessage));
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new RunBacktestInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var run = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await run.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenEndDateBeforeStartDate_ShouldThrow()
    {
        // Arrange
        var command = new RunBacktestInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow.AddDays(30),
            DateTimeOffset.UtcNow,
            10000m
        );

        // Act
        var run = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await run.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WhenBudgetIsInvalid_ShouldThrow(decimal budget)
    {
        // Arrange
        var command = new RunBacktestInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            budget
        );

        // Act
        var run = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await run.ShouldThrowAsync<ArgumentException>();
    }
}
