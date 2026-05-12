using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketOverviewBucket;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.MarketOverviewBucket;

public sealed class MarketOverviewBucketJobTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<MarketOverviewBucketJob> _logger = Substitute.For<
        ILogger<MarketOverviewBucketJob>
    >();
    private readonly PlutusDbContext _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
    private readonly TickerFunctionContext _context = new();

    public MarketOverviewBucketJobTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    private MarketOverviewBucketJob CreateJob(int numBuckets = 10) =>
        new(
            _logger,
            _dbContext,
            Options.Create(
                new MarketOverviewBucketOptions(
                    NumBuckets: numBuckets,
                    ChunkDays: 1,
                    ChunkThresholdDays: 7
                )
            )
        );

    [Fact]
    public async Task ExecuteFifteenMinutes_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteFifteenMinutes(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneHour_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteOneHour(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteFourHours_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteFourHours(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneDay_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteOneDay(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneWeek_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteOneWeek(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneMonth_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteOneMonth(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteSixMonths_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteSixMonths(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneYear_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteOneYear(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAllTime_WhenNoTrades_ShouldCreateNoBuckets()
    {
        await CreateJob().ExecuteAllTime(_context, CancellationToken.None);
        _dbContext.MarketOverviewBuckets.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAllTime_WhenTradesExist_ShouldCreateBucketsForEachMarket()
    {
        // Arrange
        var marketA = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );
        var marketB = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbolA = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            marketA.Id,
            new AdditionalFields()
        );
        var symbolB = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            marketB.Id,
            new AdditionalFields()
        );

        var now = DateTimeOffset.UtcNow;
        var tradeA1 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbolA.Id,
            100m,
            5m,
            now.AddHours(-2)
        );
        var tradeA2 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbolA.Id,
            110m,
            3m,
            now.AddHours(-1)
        );
        var tradeB1 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbolB.Id,
            200m,
            2m,
            now.AddHours(-2)
        );
        var tradeB2 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbolB.Id,
            220m,
            4m,
            now.AddHours(-1)
        );

        await _dbContext.SeedData(marketA, marketB);
        await _dbContext.SeedData(symbolA, symbolB);
        await _dbContext.SeedData(tradeA1, tradeA2, tradeB1, tradeB2);

        // Act
        await CreateJob(numBuckets: 10).ExecuteAllTime(_context, CancellationToken.None);

        // Assert
        var buckets = _dbContext.MarketOverviewBuckets.ToList();
        buckets.ShouldNotBeEmpty();
        buckets.Select(b => b.MarketId).Distinct().Count().ShouldBe(2);
        buckets.ShouldAllBe(b => b.TimeFrame == TimeFrame.AllTime);
    }

    [Fact]
    public async Task ExecuteAllTime_WhenTradesExist_BucketsShouldHaveVwapPopulated()
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
        var trade1 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            100m,
            5m,
            now.AddHours(-2)
        );
        var trade2 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            110m,
            3m,
            now.AddHours(-1)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2);

        // Act
        await CreateJob(numBuckets: 10).ExecuteAllTime(_context, CancellationToken.None);

        // Assert
        var buckets = _dbContext.MarketOverviewBuckets.ToList();
        buckets.ShouldNotBeEmpty();
        buckets.ShouldAllBe(b => b.AveragePrice > 0);
        buckets.ShouldAllBe(b => b.Volume > 0);
    }

    [Fact]
    public async Task ExecuteAllTime_WhenChunkingIsForced_ShouldCreateBucketsWithVwap()
    {
        // Arrange
        var job = new MarketOverviewBucketJob(
            _logger,
            _dbContext,
            Options.Create(
                new MarketOverviewBucketOptions(NumBuckets: 10, ChunkDays: 1, ChunkThresholdDays: 0)
            )
        );

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
        var trade1 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            100m,
            5m,
            now.AddHours(-2)
        );
        var trade2 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            110m,
            3m,
            now.AddHours(-1)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2);

        // Act
        await job.ExecuteAllTime(_context, CancellationToken.None);

        // Assert
        var buckets = _dbContext.MarketOverviewBuckets.ToList();
        buckets.ShouldNotBeEmpty();
        buckets.ShouldAllBe(b => b.AveragePrice > 0);
        buckets.ShouldAllBe(b => b.Volume > 0);
    }

    [Fact]
    public async Task ExecuteAllTime_WhenChunkingIsForced_ChunkMergeShouldCorrectlyAggregateVwap()
    {
        // Arrange
        var job = new MarketOverviewBucketJob(
            _logger,
            _dbContext,
            Options.Create(
                new MarketOverviewBucketOptions(NumBuckets: 1, ChunkDays: 1, ChunkThresholdDays: 0)
            )
        );

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

        var base1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var trade1 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            50m,
            1m,
            base1
        );
        var trade2 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            150m,
            1m,
            base1.AddHours(25)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2);

        // Act
        await job.ExecuteAllTime(_context, CancellationToken.None);

        // Assert
        var buckets = _dbContext.MarketOverviewBuckets.OrderBy(b => b.BucketStart).ToList();
        buckets.ShouldNotBeEmpty();
        buckets.ShouldAllBe(b => b.AveragePrice > 0);
        buckets.ShouldAllBe(b => b.Volume > 0);
    }

    [Fact]
    public async Task ExecuteAllTime_WhenRunTwice_ShouldReplaceExistingBuckets()
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
        var trade1 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            100m,
            5m,
            now.AddHours(-2)
        );
        var trade2 = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            110m,
            3m,
            now.AddHours(-1)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trade1, trade2);

        var job = CreateJob(numBuckets: 10);

        // Act
        await job.ExecuteAllTime(_context, CancellationToken.None);
        var countAfterFirst = _dbContext.MarketOverviewBuckets.Count();

        await job.ExecuteAllTime(_context, CancellationToken.None);
        var countAfterSecond = _dbContext.MarketOverviewBuckets.Count();

        // Assert
        countAfterSecond.ShouldBe(countAfterFirst);
    }
}
