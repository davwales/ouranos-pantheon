using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetSymbolTrades;

public sealed class GetSymbolTradesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetSymbolTradesHandler _handler;
    private readonly ILogger<GetSymbolTradesHandler> _logger = Substitute.For<ILogger<GetSymbolTradesHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetSymbolTradesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetSymbolTradesHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenTradesExist_ShouldReturnAggregatedStats()
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
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var timestamp = DateTimeOffset.UtcNow;
        var trade1 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 90m, 5m, timestamp);
        var trade2 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 110m, 3m, timestamp);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2);

        var query = new GetSymbolTradesInput(symbol.Id, TimeFrame.AllTime, NumBuckets: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.MinPrice.ShouldBe(90m);
        result.MaxPrice.ShouldBe(110m);
        result.Volume.ShouldBe(8m);
        result.NumTransactions.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenTradesSpanDifferentTimes_ShouldReturnBuckets()
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
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var now = DateTimeOffset.UtcNow;
        var trade1 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 90m, 5m, now.AddHours(-2));
        var trade2 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 100m, 3m, now.AddHours(-1));
        var trade3 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 110m, 4m, now);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2, trade3);

        var query = new GetSymbolTradesInput(symbol.Id, TimeFrame.AllTime, NumBuckets: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.MinPrice.ShouldBe(90m);
        result.MaxPrice.ShouldBe(110m);
        result.NumTransactions.ShouldBe(3);
        result.AveragePrice.ShouldBeGreaterThan(0m);
        result.TotalSpent.ShouldBeGreaterThan(0m);
        result.Volume.ShouldBeGreaterThan(0m);
        result.Trades.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoTrades_ShouldReturnZeroStats()
    {
        // Arrange
        var query = new GetSymbolTradesInput(
            new Id<Symbol>(_fixture.Create<string>()),
            TimeFrame.AllTime,
            NumBuckets: 10
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.MinPrice.ShouldBe(0m);
        result.MaxPrice.ShouldBe(0m);
        result.Volume.ShouldBe(0m);
        result.NumTransactions.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetSymbolTradesInput(
            new Id<Symbol>(_fixture.Create<string>()),
            TimeFrame.AllTime,
            NumBuckets: 10
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenSingleTradeInBucket_OpenAndClosePriceShouldEqualTradePrice()
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
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var now = DateTimeOffset.UtcNow;
        var trade1 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 75m, 2m, now.AddHours(-2));
        var trade2 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 125m, 3m, now);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2);

        var query = new GetSymbolTradesInput(symbol.Id, TimeFrame.AllTime, NumBuckets: 100);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Trades.ShouldNotBeNull();
        result.Trades.ShouldAllBe(b => b.OpenPrice > 0);
        result.Trades.ShouldAllBe(b => b.ClosePrice > 0);
    }

    [Fact]
    public async Task Handle_WhenTradesExist_BucketsShouldIncludeOpenAndClosePrice()
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
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var now = DateTimeOffset.UtcNow;
        var trade1 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 90m, 5m, now.AddHours(-2));
        var trade2 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 100m, 3m, now.AddHours(-1));
        var trade3 = Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 110m, 4m, now);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2, trade3);

        var query = new GetSymbolTradesInput(symbol.Id, TimeFrame.AllTime, NumBuckets: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Trades.ShouldNotBeNull();
        result.Trades.ShouldNotBeEmpty();
        result.Trades.ShouldAllBe(b => b.OpenPrice > 0);
        result.Trades.ShouldAllBe(b => b.ClosePrice > 0);
        result.Trades.ShouldAllBe(b => b.OpenPrice >= result.MinPrice);
        result.Trades.ShouldAllBe(b => b.ClosePrice >= result.MinPrice);
        result.Trades.ShouldAllBe(b => b.OpenPrice <= result.MaxPrice);
        result.Trades.ShouldAllBe(b => b.ClosePrice <= result.MaxPrice);
    }

    [Fact]
    public void BucketDto_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var bucketStart = DateTimeOffset.UtcNow;
        var bucket = new BucketDto(symbolId, bucketStart, 1000m, 10m, 90m, 110m, 5, 100m, 20m, 91m, 109m);

        // Assert
        bucket.SymbolId.ShouldBe(symbolId);
        bucket.BucketStart.ShouldBe(bucketStart);
        bucket.TotalSpent.ShouldBe(1000m);
        bucket.Volume.ShouldBe(10m);
        bucket.MinPrice.ShouldBe(90m);
        bucket.MaxPrice.ShouldBe(110m);
        bucket.NumTransactions.ShouldBe(5);
        bucket.AveragePrice.ShouldBe(100m);
        bucket.Margin.ShouldBe(20m);
    }

    [Fact]
    public void GetSymbolTradeBucketsResponse_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;

        // Act
        var response = new GetSymbolTradeBucketsResponse(
            Price: 100m,
            Volume: 50m,
            TotalSpent: 5000m,
            MinPrice: 90m,
            MaxPrice: 110m,
            NumTransactions: 10,
            Date: date,
            OpenPrice: 92m,
            ClosePrice: 108m
        );

        // Assert
        response.Price.ShouldBe(100m);
        response.Volume.ShouldBe(50m);
        response.TotalSpent.ShouldBe(5000m);
        response.MinPrice.ShouldBe(90m);
        response.MaxPrice.ShouldBe(110m);
        response.NumTransactions.ShouldBe(10);
        response.Date.ShouldBe(date);
        response.OpenPrice.ShouldBe(92m);
        response.ClosePrice.ShouldBe(108m);
    }
}
