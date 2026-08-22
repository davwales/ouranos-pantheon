using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.CreatePosition;

public sealed class CreatePositionHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreatePositionHandler _handler;
    private readonly ILogger<CreatePositionHandler> _logger = Substitute.For<
        ILogger<CreatePositionHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public CreatePositionHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new CreatePositionHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreatePositionAndReturnId()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.SaveChangesAsync();

        var symbolId = _fixture.Create<Id<Symbol>>();
        var command = new CreatePositionInput(PositionSide.Buy, market.Id, symbolId, 150.50m, 10m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Position>>();
        result.Id.ShouldNotBe(default);

        var position = await _dbContext.Positions.FindAsync(result.Id);
        position.ShouldNotBeNull();
        position.Side.ShouldBe(PositionSide.Buy);
        position.SymbolId.ShouldBe(symbolId);
        position.Cost.ShouldBe(150.50m);
        position.Quantity.ShouldBe(10m);
        position.MarketId.ShouldBe(market.Id);
        position.Status.ShouldBe(PositionStatus.Pending);
    }

    [Fact]
    public async Task Handle_WhenCreatingSellPosition_ShouldCreateSellWithNoLink()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        await _dbContext.Markets.AddAsync(market);
        await _dbContext.SaveChangesAsync();

        var symbolId = _fixture.Create<Id<Symbol>>();
        var command = new CreatePositionInput(PositionSide.Sell, market.Id, symbolId, 155.00m, 10m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Position>>();

        var position = await _dbContext.Positions.FindAsync(result.Id);
        position.ShouldNotBeNull();
        position.Side.ShouldBe(PositionSide.Sell);
        position.LinkedBuyPositionId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreatePositionInput(
            PositionSide.Buy,
            _fixture.Create<Id<Market>>(),
            _fixture.Create<Id<Symbol>>(),
            150.50m,
            10m
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }
}
