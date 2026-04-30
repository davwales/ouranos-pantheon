using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetAllStrategies;

public sealed class GetAllStrategiesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllStrategiesHandler _handler;
    private readonly ILogger<GetAllStrategiesHandler> _logger = Substitute.For<ILogger<GetAllStrategiesHandler>>();
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllStrategiesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _queryOptions = Options.Create(new QueryOptions());
        _handler = new GetAllStrategiesHandler(_logger, _dbContext, _queryOptions);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnStrategiesForMarket()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyType.SignalWeighted, new StrategyConfiguration());
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
    public async Task Handle_WhenNoStrategiesForMarket_ShouldReturnEmpty()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var otherMarketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(otherMarketId, "Test", null, StrategyType.SignalWeighted, new StrategyConfiguration());
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
}