using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.UpdateStrategy;

public sealed class UpdateStrategyHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateStrategyHandler _handler;
    private readonly ILogger<UpdateStrategyHandler> _logger = Substitute.For<ILogger<UpdateStrategyHandler>>();
    private readonly PlutusDbContext _dbContext;

    public UpdateStrategyHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new UpdateStrategyHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateStrategyAndReturnId()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var newConfig = new TradingConfiguration();
        var command = new UpdateStrategyInput(
            strategy.Id,
            "Updated",
            "New description",
            newConfig,
            new SignalWeightedConfig()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Strategy>>();
        result.Id.ShouldBe(strategy.Id);

        var updated = await _dbContext.Strategies.FindAsync(strategy.Id);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe("Updated");
        updated.Description.ShouldBe("New description");
    }

    [Fact]
    public async Task Handle_WhenUpdatingSignalWeightedConfig_ShouldReplaceNotDuplicate()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig(TaxAdjustedRoiWeight: 1m, VolumeAnomalyWeight: 1m)
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var newConfig = new TradingConfiguration();
        var command = new UpdateStrategyInput(
            strategy.Id,
            "Updated",
            null,
            newConfig,
            new SignalWeightedConfig(TaxAdjustedRoiWeight: 2.5m, VolumeAnomalyWeight: 0.8m)
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Strategy>>();
        result.Id.ShouldBe(strategy.Id);

        var updated = await _dbContext.Strategies
            .FirstOrDefaultAsync(s => s.Id == strategy.Id);
        updated.ShouldNotBeNull();
        updated.SignalWeightedConfig.ShouldNotBeNull();
        updated.SignalWeightedConfig.TaxAdjustedRoiWeight.ShouldBe(2.5m);
        updated.SignalWeightedConfig.VolumeAnomalyWeight.ShouldBe(0.8m);
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var command = new UpdateStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            "Updated",
            null,
            new TradingConfiguration()
        );

        // Act
        var update = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            "Updated",
            null,
            new TradingConfiguration()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
