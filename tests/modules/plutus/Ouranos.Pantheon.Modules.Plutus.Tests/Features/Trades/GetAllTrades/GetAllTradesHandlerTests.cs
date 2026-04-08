using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetAllTrades;

public sealed class GetAllTradesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllTradesHandler _handler;
    private readonly ILogger<GetAllTradesHandler> _logger = Substitute.For<ILogger<GetAllTradesHandler>>();

    public GetAllTradesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _handler = new GetAllTradesHandler(
            _logger,
            DbContextExtensions.Mock<PlutusDbContext>(),
            Options.Create(new QueryOptions())
        );
    }

    [Fact]
    public async Task Handle_WhenNoTrades_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllTradesInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllTradesInput(Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void GetAllTradesResponse_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var id = new Id<Trade>(Guid.NewGuid().ToString());
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var ts = DateTimeOffset.UtcNow;

        // Act
        var response = new GetAllTradesResponse(id, symbolId, marketId, "Gold", "AU", 100m, 5m, ts);

        // Assert
        response.Id.ShouldBe(id);
        response.SymbolId.ShouldBe(symbolId);
        response.MarketId.ShouldBe(marketId);
        response.SymbolName.ShouldBe("Gold");
        response.SymbolCode.ShouldBe("AU");
        response.Price.ShouldBe(100m);
        response.Volume.ShouldBe(5m);
        response.Timestamp.ShouldBe(ts);
    }
}
