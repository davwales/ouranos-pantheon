using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.GetPosition;

public sealed class GetPositionHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetPositionHandler _handler;
    private readonly ILogger<GetPositionHandler> _logger = Substitute.For<
        ILogger<GetPositionHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetPositionHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetPositionHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnPosition()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var symbol = Symbol.Create(
            symbolId,
            "TEST",
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
        var position = Position.Create(
            PositionSide.Buy,
            marketId,
            symbolId,
            150.50m,
            10m,
            notes: "Test position",
            symbol: symbol
        );
        await _dbContext.Symbols.AddAsync(symbol);
        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        var query = new GetPositionInput(position.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(position.Id);
        result.Side.ShouldBe(PositionSide.Buy);
        result.SymbolId.ShouldBe(symbolId);
        result.SymbolName.ShouldBe("Test Symbol");
        result.Cost.ShouldBe(150.50m);
        result.Quantity.ShouldBe(10m);
        result.MarketId.ShouldBe(marketId);
        result.Status.ShouldBe(PositionStatus.Pending);
        result.Notes.ShouldBe("Test position");
    }

    [Fact]
    public async Task Handle_WhenPositionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetPositionInput(_fixture.Create<Id<Position>>());

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetPositionInput(_fixture.Create<Id<Position>>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
