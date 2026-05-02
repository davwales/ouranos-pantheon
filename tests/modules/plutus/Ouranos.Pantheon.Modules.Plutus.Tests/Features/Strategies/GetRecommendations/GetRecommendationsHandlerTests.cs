using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetRecommendationsHandler _handler;
    private readonly ILogger<GetRecommendationsHandler> _logger = Substitute.For<ILogger<GetRecommendationsHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetRecommendationsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();

        var executor = new SignalWeightedExecutor();
        var executors = new List<IStrategyExecutor> { executor };
        var compositeExecutor = new CompositeExecutor(executors);

        _handler = new GetRecommendationsHandler(_logger, _dbContext, executors, compositeExecutor);
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var query = new GetRecommendationsInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            10000m
        );

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenMarketNotFound_ShouldThrow()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(
            strategy.Id,
            strategy.MarketId,
            10000m
        );

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenNoSymbolsForMarket_ShouldReturnEmptyResponse()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var market = Market.Create(marketId, "Test Market", new Taxes(null));
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.SaveChangesAsync();

        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetRecommendationsInput(
            strategy.Id,
            marketId,
            10000m
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetRecommendationsResponse>();
        result.Recommendations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetRecommendationsInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            10000m
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenNegativeOrZeroBudget_ShouldThrow()
    {
        // Arrange
        var query = new GetRecommendationsInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            0m
        );

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }
}
