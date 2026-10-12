using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.UpdateStrategy;

public sealed class UpdateStrategyHandlerTests
{
    private readonly Fixture _fixture = new();
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
            StrategyTestFactory.Swing()
        );
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var configuration = new SwingConfiguration(
            [
                new InputWeight(InputKind.SignalRsi, 1m),
                new InputWeight(InputKind.SignalTrendMomentum, 0.5m),
            ],
            BuyThreshold: 0.15m,
            MaxPositions: 5,
            HoldPeriodDays: 7
        );

        var command = new UpdateStrategyInput(
            strategy.Id,
            "Updated",
            "New description",
            configuration
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

        var swing = updated.Configuration.ShouldBeOfType<SwingConfiguration>();
        swing.InputWeights.Count.ShouldBe(2);
        swing.InputWeights.ShouldContain(w => w.Kind == InputKind.SignalRsi && w.Weight == 1m);
        swing.BuyThreshold.ShouldBe(0.15m);
        swing.MaxPositions.ShouldBe(5);
        swing.HoldPeriodDays.ShouldBe(7);
    }

    [Fact]
    public async Task Handle_WhenSwitchingToFlip_ShouldReplaceConfiguration()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            StrategyTestFactory.Swing()
        );

        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var configuration = new FlipConfiguration(MaxOpenOrders: 3);
        var command = new UpdateStrategyInput(strategy.Id, "Flipper", null, configuration);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Strategies.FindAsync(strategy.Id);
        updated.ShouldNotBeNull();
        updated.Configuration.ShouldBe(configuration);
        updated.Configuration.Mode.ShouldBe(StrategyMode.Flip);
    }

    [Fact]
    public async Task Handle_WhenConfigurationIsInvalid_ShouldThrowAndKeepOriginal()
    {
        // Arrange
        var original = StrategyTestFactory.Swing();
        var strategy = Strategy.Create(_fixture.Create<Id<Market>>(), "Original", null, original);
        await _dbContext.Strategies.AddAsync(strategy);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateStrategyInput(
            strategy.Id,
            "Updated",
            null,
            new FlipConfiguration(MaxOpenOrders: 0)
        );

        // Act
        var update = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<ArgumentException>();
        strategy.Name.ShouldBe("Original");
        strategy.Configuration.ShouldBe(original);
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var command = new UpdateStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            "Updated",
            null,
            StrategyTestFactory.Swing()
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
            StrategyTestFactory.Swing()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
