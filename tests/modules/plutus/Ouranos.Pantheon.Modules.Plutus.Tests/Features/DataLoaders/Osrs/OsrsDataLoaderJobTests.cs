using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.DataLoaders;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Osrs;

public sealed class OsrsDataLoaderJobTests
{
    private readonly ILogger<OsrsDataLoaderJob> _logger = Substitute.For<
        ILogger<OsrsDataLoaderJob>
    >();
    private readonly IWikiClient _wikiClient = Substitute.For<IWikiClient>();
    private readonly IQueueTradeMessages _queue = Substitute.For<IQueueTradeMessages>();
    private readonly IDbContextFactory<PlutusDbContext> _factory = Substitute.For<
        IDbContextFactory<PlutusDbContext>
    >();
    private readonly TickerFunctionContext _context = new();
    private readonly string _dbName = Guid.NewGuid().ToString();

    private OsrsDataLoaderJob CreateJob(bool isEnabled = true)
    {
        return new(
            _logger,
            _wikiClient,
            _queue,
            _factory,
            Options.Create(
                new OsrsDataLoaderOptions(
                    IsEnabled: isEnabled,
                    RefreshIntervalMinutes: 5,
                    Wiki: new OsrsWikiOptions(
                        "https://prices.runescape.wiki/api/v1/osrs/",
                        "TestAgent/1.0"
                    )
                )
            )
        );
    }

    private PlutusDbContext CreateContext()
    {
        return DbContextExtensions.Mock<PlutusDbContext>(_dbName);
    }

    private async Task SeedState(DateTimeOffset? lastProcessed = null)
    {
        await using var db = CreateContext();
        var state = new OsrsDataLoaderState
        {
            Id = OsrsDataLoaderState.SingletonId,
            LastProcessed = lastProcessed,
        };
        await db.OsrsDataLoaderStates.AddAsync(state);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_WhenDisabled_ShouldNotFetchPrices()
    {
        // Arrange
        _factory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(CreateContext()));
        var job = CreateJob(isEnabled: false);

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        await _wikiClient.DidNotReceive().GetPrices(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WhenNewTimestamp_ShouldQueueTradesAndSaveState()
    {
        // Arrange
        await SeedState(lastProcessed: null);

        _factory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(CreateContext()));

        var timestamp = DateTimeOffset.UtcNow;
        var priceResponse = new PriceResponse(
            Data: new Dictionary<string, Price> { ["1234"] = new Price(500, 10, 450, 8) },
            Timestamp: (int)timestamp.ToUnixTimeSeconds()
        );

        _wikiClient.GetPrices(Arg.Any<CancellationToken>()).Returns(Task.FromResult(priceResponse));
        _wikiClient
            .GetMappings(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new List<Mapping>
                    {
                        new(
                            1234,
                            "Test Item",
                            "test.png",
                            "A test item",
                            true,
                            null,
                            null,
                            100,
                            1000
                        ),
                    }
                )
            );

        var job = CreateJob();

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        await _queue
            .Received()
            .QueueMessages(
                Arg.Is<IReadOnlyCollection<TradeMessage>>(msgs =>
                    msgs.Count == 2
                    && msgs.DistinctBy(t => t.TradeId).Count() == 2
                    && msgs.Any(t => t.Price == 450m && t.Volume == 8m)
                    && msgs.Any(t => t.Price == 500m && t.Volume == 10m)
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Execute_WhenPriceExceedsInt32_ShouldQueueTradeWithFullValue()
    {
        // Arrange
        await SeedState(lastProcessed: null);

        _factory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(CreateContext()));

        var timestamp = DateTimeOffset.UtcNow;
        var priceResponse = new PriceResponse(
            Data: new Dictionary<string, Price>
            {
                ["1234"] = new Price(3_500_000_000, 10, 3_450_000_000, 8),
            },
            Timestamp: (int)timestamp.ToUnixTimeSeconds()
        );

        _wikiClient.GetPrices(Arg.Any<CancellationToken>()).Returns(Task.FromResult(priceResponse));
        _wikiClient
            .GetMappings(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new List<Mapping>
                    {
                        new(
                            1234,
                            "Test Item",
                            "test.png",
                            "A test item",
                            true,
                            2_500_000_000,
                            3_000_000_000,
                            100,
                            5_000_000_000
                        ),
                    }
                )
            );

        var job = CreateJob();

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        await _queue
            .Received()
            .QueueMessages(
                Arg.Is<IReadOnlyCollection<TradeMessage>>(msgs =>
                    msgs.Count == 2
                    && msgs.Any(t => t.Price == 3_500_000_000m)
                    && msgs.Any(t => t.Price == 3_450_000_000m)
                    && msgs.All(t =>
                        t.AdditionalFields.HighAlch == 3_000_000_000m
                        && t.AdditionalFields.LowAlch == 2_500_000_000m
                        && t.AdditionalFields.Limit == 100m
                    )
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
