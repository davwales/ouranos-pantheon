using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.DeleteStrategy;

public sealed class DeleteStrategyHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteStrategyHandler _handler;
    private readonly ILogger<DeleteStrategyHandler> _logger = Substitute.For<ILogger<DeleteStrategyHandler>>();
    private readonly PlutusDbContext _dbContext;

    public DeleteStrategyHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new DeleteStrategyHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenStrategyExists_ShouldDelete()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var strategy = Strategy.Create(marketId, "Test", null, StrategyType.SignalWeighted, new StrategyConfiguration());
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteStrategyInput(strategy.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _dbContext.Strategies.FindAsync(strategy.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldStillReturnId()
    {
        // Arrange
        var command = new DeleteStrategyInput(_fixture.Create<Id<Strategy>>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Strategy>>();
        result.Id.ShouldBe(command.StrategyId);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteStrategyInput(_fixture.Create<Id<Strategy>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var delete = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await delete.ShouldThrowAsync<OperationCanceledException>();
    }
}