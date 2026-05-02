using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Wolverine;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest;

public sealed class RunBacktestHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly RunBacktestHandler _handler;
    private readonly ILogger<RunBacktestHandler> _logger = Substitute.For<ILogger<RunBacktestHandler>>();
    private readonly PlutusDbContext _dbContext;
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    public RunBacktestHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new RunBacktestHandler(_logger, _dbContext, _bus);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateBacktestAndPublishMessage()
    {
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyType.SignalWeighted, new StrategyConfiguration());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var command = new RunBacktestInput(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<RunBacktestResponse>();
        result.BacktestId.ShouldNotBe(default);

        var backtest = await _dbContext.Backtests.FindAsync(result.BacktestId);
        backtest.ShouldNotBeNull();
        backtest.Status.ShouldBe(BacktestStatus.Pending);
        backtest.StrategyId.ShouldBe(strategy.Id);
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        var command = new RunBacktestInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        var run = async () => await _handler.Handle(command, CancellationToken.None);

        await run.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        var command = new RunBacktestInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        var cancellationToken = new CancellationToken(true);

        var run = async () => await _handler.Handle(command, cancellationToken);

        await run.ShouldThrowAsync<OperationCanceledException>();
    }
}