using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Wolverine;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly OptimizeStrategyHandler _handler;
    private readonly ILogger<OptimizeStrategyHandler> _logger = Substitute.For<
        ILogger<OptimizeStrategyHandler>
    >();
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    public OptimizeStrategyHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
        _handler = new OptimizeStrategyHandler(_logger, _dbContextFactory, _bus);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateBacktestAndPublishOptimizeMessage()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            10,
            5
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OptimizeStrategyResponse>();
        result.BacktestId.ShouldNotBe(default);
        await _bus.Received(1)
            .PublishAsync(
                Arg.Any<Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events.OptimizeStrategyMessage>()
            );
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrow()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenGenerationsExceedsMaximum_ShouldThrow()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            Generations: 501,
            PopulationSize: 10
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenPopulationSizeBelowMinimum_ShouldThrow()
    {
        // Arrange
        await using var setupContext = await _dbContextFactory.CreateDbContextAsync();
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        await setupContext.Strategies.AddAsync(strategy);
        await setupContext.SaveChangesAsync();

        var command = new OptimizeStrategyInput(
            strategy.Id,
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m,
            Generations: 10,
            PopulationSize: 1
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenBudgetIsZero_ShouldThrow()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            0m
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenEndDateBeforeStartDate_ShouldThrow()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m
        );

        // Act
        var optimize = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await optimize.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new OptimizeStrategyInput(
            _fixture.Create<Id<Strategy>>(),
            _fixture.Create<Id<Market>>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            10000m
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var optimize = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await optimize.ShouldThrowAsync<OperationCanceledException>();
    }
}
