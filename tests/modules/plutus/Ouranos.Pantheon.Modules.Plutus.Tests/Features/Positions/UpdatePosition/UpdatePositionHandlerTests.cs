using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.UpdatePosition;

public sealed class UpdatePositionHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdatePositionHandler _handler;
    private readonly ILogger<UpdatePositionHandler> _logger = Substitute.For<
        ILogger<UpdatePositionHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public UpdatePositionHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new UpdatePositionHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdatePositionAndReturnId()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m,
            notes: "Original note"
        );
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new UpdatePositionInput(position.Id, 200.00m, 5m, "Updated note");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Position>>();
        result.Id.ShouldBe(position.Id);

        var updated = await _dbContext.Positions.FindAsync(position.Id);
        updated.ShouldNotBeNull();
        updated.Cost.ShouldBe(200.00m);
        updated.Quantity.ShouldBe(5m);
        updated.Notes.ShouldBe("Updated note");
    }

    [Fact]
    public async Task Handle_WhenPositionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdatePositionInput(_fixture.Create<Id<Position>>(), 200.00m, 5m, null);

        // Act
        var update = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPositionNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var position = Position.Create(PositionSide.Buy, marketId, symbolId, 150.50m, 10m);
        position.Close(PositionStatus.Bought);
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var command = new UpdatePositionInput(position.Id, 200.00m, 5m, null);

        // Act
        var update = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdatePositionInput(_fixture.Create<Id<Position>>(), 200.00m, 5m, null);
        var cancellationToken = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
