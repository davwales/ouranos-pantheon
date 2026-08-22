using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.UpdateStrategy;

public sealed class UpdateStrategyHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateStrategyHandler _handler;
    private readonly ILogger<UpdateStrategyHandler> _logger = Substitute.For<
        ILogger<UpdateStrategyHandler>
    >();
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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var newConfig = new TradingConfiguration();
        var newWeights = new List<InputWeight>
        {
            new(InputKind.SignalRsi, 1m),
            new(InputKind.SignalTrendMomentum, 0.5m),
        };
        var command = new UpdateStrategyInput(
            strategy.Id,
            "Updated",
            "New description",
            newConfig,
            newWeights,
            new InputThresholds(BuyThreshold: 0.15m)
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
        updated.TradingConfiguration.ShouldBe(newConfig);
        updated.InputWeights.Count.ShouldBe(2);
        updated.InputWeights.ShouldContain(w => w.Kind == InputKind.SignalRsi && w.Weight == 1m);
        updated.InputWeights.ShouldContain(w =>
            w.Kind == InputKind.SignalTrendMomentum && w.Weight == 0.5m
        );
        updated.Thresholds.BuyThreshold.ShouldBe(0.15m);
    }

    [Fact]
    public async Task Handle_WhenUpdatingInputWeights_ShouldReplaceNotDuplicate()
    {
        // Arrange
        var originalWeights = new List<InputWeight>
        {
            new(InputKind.SignalTaxAdjustedRoi, 1m),
            new(InputKind.SignalVolumeAnomaly, 1m),
        };
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            new TradingConfiguration(),
            originalWeights,
            null
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var newConfig = new TradingConfiguration();
        var newWeights = new List<InputWeight>
        {
            new(InputKind.SignalTaxAdjustedRoi, 2.5m),
            new(InputKind.SignalVolumeAnomaly, 0.8m),
        };
        var command = new UpdateStrategyInput(
            strategy.Id,
            "Updated",
            null,
            newConfig,
            newWeights,
            null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Strategy>>();
        result.Id.ShouldBe(strategy.Id);

        var updated = await _dbContext.Strategies.FirstOrDefaultAsync(s => s.Id == strategy.Id);
        updated.ShouldNotBeNull();
        updated.InputWeights.Count.ShouldBe(2);
        updated.InputWeights.ShouldContain(w =>
            w.Kind == InputKind.SignalTaxAdjustedRoi && w.Weight == 2.5m
        );
        updated.InputWeights.ShouldContain(w =>
            w.Kind == InputKind.SignalVolumeAnomaly && w.Weight == 0.8m
        );
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var command = new UpdateStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            "Updated",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights()
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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
