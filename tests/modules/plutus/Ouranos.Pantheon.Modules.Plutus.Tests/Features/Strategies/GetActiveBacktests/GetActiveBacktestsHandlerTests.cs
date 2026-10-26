using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetActiveBacktests;

public sealed class GetActiveBacktestsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetActiveBacktestsHandler _handler;
    private readonly ILogger<GetActiveBacktestsHandler> _logger = Substitute.For<
        ILogger<GetActiveBacktestsHandler>
    >();
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<QueryOptions> _queryOptions = Substitute.For<
        IOptions<QueryOptions>
    >();

    public GetActiveBacktestsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();

        _queryOptions.Value.Returns(
            new QueryOptions
            {
                MaxSkip = 1000,
                MinPageSize = 1,
                MaxPageSize = 100,
            }
        );

        _handler = new GetActiveBacktestsHandler(_logger, _dbContext, _queryOptions);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        ILogger<GetActiveBacktestsHandler> logger = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetActiveBacktestsHandler(logger, _dbContext, _queryOptions)
        );
    }

    [Fact]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        PlutusDbContext dbContext = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetActiveBacktestsHandler(_logger, dbContext, _queryOptions)
        );
    }

    [Fact]
    public void Constructor_WhenQueryOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IOptions<QueryOptions> queryOptions = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetActiveBacktestsHandler(_logger, _dbContext, queryOptions)
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnOnlyPendingAndRunningBacktests()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = await SeedStrategyAsync(marketId, "Swing Alpha");

        var pending = CreateBacktest(strategy, marketId);
        var running = CreateBacktest(strategy, marketId);
        running.MarkRunning();
        var cancelled = CreateBacktest(strategy, marketId);
        cancelled.Cancel();
        var failed = CreateBacktest(strategy, marketId);
        failed.MarkRunning();
        failed.Fail("boom");

        await _dbContext.Backtests.AddRangeAsync(pending, running, cancelled, failed);
        await _dbContext.SaveChangesAsync();

        var input = new GetActiveBacktestsInput();

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(2);
        result.Items.Select(b => b.Id).ShouldBe([pending.Id, running.Id], ignoreOrder: true);
        result.Items.ShouldAllBe(b => b.StrategyName == "Swing Alpha");
        result.Items.ShouldAllBe(b => b.StrategyId == strategy.Id);
    }

    [Fact]
    public async Task Handle_WhenMarketIdProvided_ShouldReturnOnlyThatMarketsBacktests()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var otherMarketId = _fixture.Create<Id<Market>>();
        var strategy = await SeedStrategyAsync(marketId, "Mine");
        var otherStrategy = await SeedStrategyAsync(otherMarketId, "Theirs");

        var mine = CreateBacktest(strategy, marketId);
        var theirs = CreateBacktest(otherStrategy, otherMarketId);

        await _dbContext.Backtests.AddRangeAsync(mine, theirs);
        await _dbContext.SaveChangesAsync();

        var input = new GetActiveBacktestsInput(marketId);

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Single().Id.ShouldBe(mine.Id);
    }

    [Fact]
    public async Task Handle_WhenPaged_ShouldReturnTotalCountAndRequestedPage()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = await SeedStrategyAsync(marketId, "Paged");

        await _dbContext.Backtests.AddRangeAsync(
            CreateBacktest(strategy, marketId),
            CreateBacktest(strategy, marketId),
            CreateBacktest(strategy, marketId)
        );
        await _dbContext.SaveChangesAsync();

        var input = new GetActiveBacktestsInput(Skip: 1, Take: 1);

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(3);
        result.Items.Count().ShouldBe(1);
        result.Skip.ShouldBe(1);
        result.Take.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenTakeExceedsMaxPageSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var input = new GetActiveBacktestsInput(Take: 101);

        // Act & Assert
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() =>
            _handler.Handle(input, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var input = new GetActiveBacktestsInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var run = async () => await _handler.Handle(input, cancellationToken);

        // Assert
        await run.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<Strategy> SeedStrategyAsync(Id<Market> marketId, string name)
    {
        var strategy = Strategy.Create(marketId, name, null, StrategyTestFactory.Swing());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();
        return strategy;
    }

    private static Backtest CreateBacktest(Strategy strategy, Id<Market> marketId)
    {
        return Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            new BacktestRunSettings()
        );
    }
}
