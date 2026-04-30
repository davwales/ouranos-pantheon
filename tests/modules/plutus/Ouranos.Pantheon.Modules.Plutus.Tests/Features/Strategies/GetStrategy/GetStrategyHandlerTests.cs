using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetStrategy;

public sealed class GetStrategyHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetStrategyHandler _handler;
    private readonly ILogger<GetStrategyHandler> _logger = Substitute.For<ILogger<GetStrategyHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetStrategyHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetStrategyHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenStrategyExists_ShouldReturnStrategy()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test Strategy", "Description", StrategyType.SignalWeighted, new StrategyConfiguration());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var query = new GetStrategyInput(strategy.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(strategy.Id);
        result.Name.ShouldBe("Test Strategy");
        result.Description.ShouldBe("Description");
        result.Type.ShouldBe(StrategyType.SignalWeighted);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var query = new GetStrategyInput(_fixture.Create<Id<Strategy>>());

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetStrategyInput(_fixture.Create<Id<Strategy>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}