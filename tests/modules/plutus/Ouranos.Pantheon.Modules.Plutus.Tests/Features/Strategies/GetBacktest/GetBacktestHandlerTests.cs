using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetBacktest;

public sealed class GetBacktestHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetBacktestHandler _handler;
    private readonly ILogger<GetBacktestHandler> _logger = Substitute.For<ILogger<GetBacktestHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetBacktestHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetBacktestHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnBacktest()
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
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        await _dbContext.Backtests.AddAsync(backtest);
        await _dbContext.SaveChangesAsync();

        var query = new GetBacktestInput(backtest.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(backtest.Id);
        result.StrategyId.ShouldBe(strategy.Id);
        result.MarketId.ShouldBe(marketId);
        result.Status.ShouldBe(BacktestStatus.Pending);
        result.Kind.ShouldBe(BacktestKind.Backtest);
        result.Budget.ShouldBe(10000m);
        result.ProgressPercent.ShouldBe(0);
        result.ProgressMessage.ShouldBeNull();
        result.Results.ShouldBeNull();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenBacktestNotFound_ShouldThrow()
    {
        // Arrange
        var query = new GetBacktestInput(_fixture.Create<Id<Backtest>>());

        // Act
        var run = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await run.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetBacktestInput(_fixture.Create<Id<Backtest>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var run = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await run.ShouldThrowAsync<OperationCanceledException>();
    }
}