using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;
using Snapshot = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades.MarketTradeSnapshot;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetMarketTrades;

public sealed class GetMarketTradesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetMarketTradesHandler _handler;
    private readonly ILogger<GetMarketTradesHandler> _logger = Substitute.For<
        ILogger<GetMarketTradesHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetMarketTradesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetMarketTradesHandler(
            _logger,
            _dbContext,
            Options.Create(new QueryOptions())
        );
    }

    [Fact]
    public async Task Handle_WhenSnapshotsExist_ShouldReturnPagedResponse()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            "NQ",
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var snapshot = new Snapshot(
            market.Id,
            symbol.Id,
            TimeFrame.OneDay,
            TotalSpent: 1000m,
            MinPrice: 90m,
            MaxPrice: 110m,
            TotalVolume: 10m,
            NumTransactions: 5,
            Limit: 100m,
            Tax: 1m
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(snapshot);

        var query = new GetMarketTradesInput(market.Id, TimeFrame.OneDay, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenNoSnapshots_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetMarketTradesInput(
            new Id<Market>(_fixture.Create<string>()),
            TimeFrame.OneDay,
            Take: 10
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetMarketTradesInput(
            new Id<Market>(_fixture.Create<string>()),
            TimeFrame.OneDay,
            Take: 10
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void GetMarketTradesResponse_ComputedProperties_ShouldCalculateCorrectly()
    {
        // Arrange
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var response = new GetMarketTradesResponse(
            symbolId,
            "Symbol",
            "NQ",
            TotalSpent: 1000m,
            MinPrice: 90m,
            MaxPrice: 110m,
            TotalVolume: 10m,
            NumTransactions: 5,
            Limit: 100m,
            Tax: 1m
        );

        // Act & Assert
        response.SymbolId.ShouldBe(symbolId);
        response.SymbolName.ShouldBe("Symbol");
        response.SymbolSubcode.ShouldBe("NQ");
        response.TotalSpent.ShouldBe(1000m);
        response.MinPrice.ShouldBe(90m);
        response.MaxPrice.ShouldBe(110m);
        response.TotalVolume.ShouldBe(10m);
        response.NumTransactions.ShouldBe(5);
        response.Limit.ShouldBe(100m);
        response.Tax.ShouldBe(1m);
        response.Margin.ShouldBe(110m - 90m - 1m);
        response.AveragePrice.ShouldBe(1000m / 10m);
        response.Roi.ShouldBe((110m - 90m - 1m) / 90m);
        response.TotalGain.ShouldBe(19m * 10m);
    }
}
