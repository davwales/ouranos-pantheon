using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetAllStrategies;

public sealed class GetAllStrategiesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllStrategiesHandler _handler;
    private readonly ILogger<GetAllStrategiesHandler> _logger = Substitute.For<
        ILogger<GetAllStrategiesHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetAllStrategiesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllStrategiesHandler(
            _logger,
            _dbContext,
            Options.Create(new QueryOptions())
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnStrategiesForMarket()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllStrategiesInput(marketId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldNotBeEmpty();
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenStrategyHasCompletedBacktests_ShouldReturnLatestBacktestBudgetAndDate()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing());
        var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddRangeAsync(
            CreateBacktest(strategy, 1000m, completed: true).WithCreatedAt(baseTime),
            CreateBacktest(strategy, 2500m, completed: true).WithCreatedAt(baseTime.AddDays(1)),
            CreateBacktest(strategy, 9999m, completed: false).WithCreatedAt(baseTime.AddDays(2))
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetAllStrategiesInput(marketId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var item = result.Items.ShouldHaveSingleItem();
        item.BacktestCount.ShouldBe(2);
        item.LastBacktestBudget.ShouldBe(2500m);
        item.LastBacktestAt.ShouldBe(baseTime.AddDays(1));
    }

    [Fact]
    public async Task Handle_WhenStrategyHasNoCompletedBacktests_ShouldReturnNullLatestBacktest()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.Backtests.AddAsync(CreateBacktest(strategy, 1000m, completed: false));
        await _dbContext.SaveChangesAsync();

        var query = new GetAllStrategiesInput(marketId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var item = result.Items.ShouldHaveSingleItem();
        item.BacktestCount.ShouldBe(0);
        item.LastBacktestBudget.ShouldBeNull();
        item.LastBacktestAt.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoStrategiesForMarket_ShouldReturnEmpty()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var otherMarketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(otherMarketId, "Test", null, StrategyTestFactory.Swing());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllStrategiesInput(marketId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllStrategiesInput(_fixture.Create<Id<Market>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    private static Backtest CreateBacktest(Strategy strategy, decimal budget, bool completed)
    {
        var backtest = Backtest.Create(
            strategy.Id,
            strategy.MarketId,
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow,
            budget,
            new BacktestRunSettings(),
            strategy
        );

        if (completed)
        {
            backtest.MarkRunning();
            backtest.Complete(new BacktestResults());
        }

        return backtest;
    }
}
