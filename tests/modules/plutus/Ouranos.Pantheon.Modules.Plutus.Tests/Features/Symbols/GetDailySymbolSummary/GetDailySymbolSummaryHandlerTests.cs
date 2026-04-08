using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.GetDailySymbolSummary;

public sealed class GetDailySymbolSummaryHandlerTests
{
    private readonly ILogger<GetDailySymbolSummaryHandler> _logger =
        Substitute.For<ILogger<GetDailySymbolSummaryHandler>>();

    private readonly PlutusDbContext _dbContext;
    private readonly GetDailySymbolSummaryHandler _handler;

    public GetDailySymbolSummaryHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetDailySymbolSummaryHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenTradesExistToday_ShouldReturnAggregatedSummary()
    {
        // Arrange
        var market = Market.Create(new Id<Market>(Guid.NewGuid().ToString()), "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "ITEM1",
            null,
            "Item One",
            market.Id,
            new AdditionalFields()
        );

        var today = DateTimeOffset.UtcNow;
        var trades = new[]
        {
            Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 100m, 10m, today),
            Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 200m, 5m, today),
        };

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trades);

        var query = new GetDailySymbolSummaryInput(symbol.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.MinPrice.ShouldBe(100m);
        result.MaxPrice.ShouldBe(200m);
        result.Volume.ShouldBe(15m);
        result.AveragePrice.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task Handle_WhenNoTradesToday_ShouldReturnZeroedSummary()
    {
        // Arrange
        var market = Market.Create(new Id<Market>(Guid.NewGuid().ToString()), "Test Market", new Taxes(null));
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "ITEM1",
            null,
            "Item One",
            market.Id,
            new AdditionalFields()
        );

        var yesterday = DateTimeOffset.UtcNow.AddDays(-2);
        var trade = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 100m, 10m, yesterday);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade);

        var query = new GetDailySymbolSummaryInput(symbol.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(new GetDailySymbolSummaryResponse(0, 0, 0, 0));
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var query = new GetDailySymbolSummaryInput(symbolId);
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
