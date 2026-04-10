using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using Bucket = Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades.MarketOverviewBucket;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetMarketOverview;

public sealed class GetMarketOverviewHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetMarketOverviewHandler _handler;
    private readonly ILogger<GetMarketOverviewHandler> _logger = Substitute.For<ILogger<GetMarketOverviewHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetMarketOverviewHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetMarketOverviewHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenBucketsExist_ShouldReturnBucketsForMarketAndTimeFrame()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var otherMarketId = new Id<Market>(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var bucket1 = Bucket.Create(marketId, TimeFrame.OneHour, now.AddHours(-2), 100m, 90m, 110m, 5m, 500m, 10);
        var bucket2 = Bucket.Create(marketId, TimeFrame.OneHour, now.AddHours(-1), 120m, 100m, 130m, 8m, 960m, 15);
        var wrongFrame = Bucket.Create(marketId, TimeFrame.OneDay, now.AddDays(-1), 50m, 40m, 60m, 3m, 150m, 5);
        var wrongMarket = Bucket.Create(otherMarketId, TimeFrame.OneHour, now.AddHours(-1), 999m, 900m, 1000m, 1m, 999m, 1);

        await _dbContext.SeedData(bucket1, bucket2, wrongFrame, wrongMarket);

        var query = new GetMarketOverviewInput(marketId, TimeFrame.OneHour);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Trades.Count.ShouldBe(2);
        result.Trades[0].Date.ShouldBe(now.AddHours(-2));
        result.Trades[1].Date.ShouldBe(now.AddHours(-1));
    }

    [Fact]
    public async Task Handle_WhenBucketsExist_ShouldAggregateStatsAcrossBuckets()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;
        var bucket1 = Bucket.Create(marketId, TimeFrame.AllTime, now.AddDays(-2), 100m, 90m, 110m, 4m, 400m, 8);
        var bucket2 = Bucket.Create(marketId, TimeFrame.AllTime, now.AddDays(-1), 200m, 180m, 220m, 1m, 200m, 2);

        await _dbContext.SeedData(bucket1, bucket2);

        var query = new GetMarketOverviewInput(marketId, TimeFrame.AllTime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.MinPrice.ShouldBe(90m);
        result.MaxPrice.ShouldBe(220m);
        result.Volume.ShouldBe(5m);
        result.TotalSpent.ShouldBe(600m);
        result.AveragePrice.ShouldBe(120m);
        result.NumTransactions.ShouldBe(10);
    }

    [Fact]
    public async Task Handle_WhenNoBuckets_ShouldReturnZeroStats()
    {
        // Arrange
        var query = new GetMarketOverviewInput(
            new Id<Market>(_fixture.Create<string>()),
            TimeFrame.OneHour
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.MinPrice.ShouldBe(0m);
        result.MaxPrice.ShouldBe(0m);
        result.Volume.ShouldBe(0m);
        result.NumTransactions.ShouldBe(0);
        result.Trades.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetMarketOverviewInput(
            new Id<Market>(_fixture.Create<string>()),
            TimeFrame.OneHour
        );

        // Act
        var get = async () => await _handler.Handle(query, new CancellationToken(true));

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void GetMarketOverviewBucketResponse_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;

        // Act
        var response = new GetMarketOverviewBucketResponse(
            Price: 100m,
            Volume: 50m,
            TotalSpent: 5000m,
            MinPrice: 90m,
            MaxPrice: 110m,
            NumTransactions: 10,
            Date: date
        );

        // Assert
        response.Price.ShouldBe(100m);
        response.Volume.ShouldBe(50m);
        response.TotalSpent.ShouldBe(5000m);
        response.MinPrice.ShouldBe(90m);
        response.MaxPrice.ShouldBe(110m);
        response.NumTransactions.ShouldBe(10);
        response.Date.ShouldBe(date);
    }
}
