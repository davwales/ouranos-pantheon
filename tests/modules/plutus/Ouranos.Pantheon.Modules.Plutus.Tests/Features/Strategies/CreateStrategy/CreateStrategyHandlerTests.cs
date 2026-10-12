using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.CreateStrategy;

public sealed class CreateStrategyHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateStrategyHandler _handler;
    private readonly ILogger<CreateStrategyHandler> _logger = Substitute.For<
        ILogger<CreateStrategyHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public CreateStrategyHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new CreateStrategyHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateStrategyAndReturnId()
    {
        // Arrange
        var configuration = StrategyTestFactory.Swing();
        var command = new CreateStrategyInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            configuration
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Strategy>>();
        result.Id.ShouldNotBe(default);

        var strategy = await _dbContext.Strategies.FindAsync(result.Id);
        strategy.ShouldNotBeNull();
        strategy.Name.ShouldBe(command.Name);
        strategy.MarketId.ShouldBe(command.MarketId);
        strategy.IsActive.ShouldBeTrue();
        var swing = strategy.Configuration.ShouldBeOfType<SwingConfiguration>();
        swing.InputWeights.ShouldBe(configuration.InputWeights);
    }

    [Fact]
    public async Task Handle_WhenFlipConfiguration_ShouldPersistFlipConfiguration()
    {
        // Arrange
        var configuration = new FlipConfiguration(
            CheckInHours: 4,
            MaxOpenOrders: 6,
            MinMargin: 0.05m
        );

        var command = new CreateStrategyInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            configuration
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var strategy = await _dbContext.Strategies.FindAsync(result.Id);
        strategy.ShouldNotBeNull();
        strategy.Configuration.ShouldBe(configuration);
        strategy.Configuration.Mode.ShouldBe(StrategyMode.Flip);
    }

    [Fact]
    public async Task Handle_WhenSwingConfigurationIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateStrategyInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            new SwingConfiguration([])
        );

        // Act
        var create = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await create.ShouldThrowAsync<ArgumentException>();
        _dbContext.Strategies.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDescriptionIsNull_ShouldCreateWithoutDescription()
    {
        // Arrange
        var command = new CreateStrategyInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            StrategyTestFactory.Swing()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var strategy = await _dbContext.Strategies.FindAsync(result.Id);
        strategy.ShouldNotBeNull();
        strategy.Description.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateStrategyInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            StrategyTestFactory.Swing()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }
}
