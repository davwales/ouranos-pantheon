using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.SetStrategyActive;

public sealed class SetStrategyActiveHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly SetStrategyActiveHandler _handler;
    private readonly ILogger<SetStrategyActiveHandler> _logger = Substitute.For<ILogger<SetStrategyActiveHandler>>();
    private readonly PlutusDbContext _dbContext;

    public SetStrategyActiveHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new SetStrategyActiveHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenDeactivating_ShouldSetIsActiveFalse()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var command = new SetStrategyActiveInput(strategy.Id, false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Strategies.FindAsync(strategy.Id);
        updated.ShouldNotBeNull();
        updated.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenActivating_ShouldSetIsActiveTrue()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyType.SignalWeighted,
            new StrategyConfiguration()
        );
        strategy.SetActive(false);
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var command = new SetStrategyActiveInput(strategy.Id, true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Strategies.FindAsync(strategy.Id);
        updated.ShouldNotBeNull();
        updated.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var command = new SetStrategyActiveInput(_fixture.Create<Id<Strategy>>(), true);

        // Act
        var toggle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await toggle.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new SetStrategyActiveInput(_fixture.Create<Id<Strategy>>(), true);
        var cancellationToken = new CancellationToken(true);

        // Act
        var toggle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await toggle.ShouldThrowAsync<OperationCanceledException>();
    }
}