using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetAllBacktests;

public sealed class GetAllBacktestsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllBacktestsHandler _handler;
    private readonly ILogger<GetAllBacktestsHandler> _logger = Substitute.For<
        ILogger<GetAllBacktestsHandler>
    >();
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<QueryOptions> _queryOptions = Substitute.For<
        IOptions<QueryOptions>
    >();

    public GetAllBacktestsHandlerTests()
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
        _handler = new GetAllBacktestsHandler(_logger, _dbContext, _queryOptions);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnBacktestsForStrategy()
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

        var backtest1 = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        var backtest2 = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(60),
            20000m
        );
        await _dbContext.Backtests.AddRangeAsync(backtest1, backtest2);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllBacktestsInput(strategy.Id, Skip: 0, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count().ShouldBe(2);
        result.TotalCount.ShouldBe(2);
        result.Skip.ShouldBe(0);
        result.Take.ShouldBe(10);
    }

    [Fact]
    public async Task Handle_WhenNoBacktests_ShouldReturnEmptyResponse()
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

        var query = new GetAllBacktestsInput(strategy.Id, Skip: 0, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllBacktestsInput(_fixture.Create<Id<Strategy>>(), Skip: 0, Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var run = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await run.ShouldThrowAsync<OperationCanceledException>();
    }
}
