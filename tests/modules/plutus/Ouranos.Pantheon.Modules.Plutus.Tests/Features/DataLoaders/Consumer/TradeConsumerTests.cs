using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Consumer;

public sealed class TradeConsumerTests
{
    private readonly ILogger<TradeConsumer> _logger = Substitute.For<ILogger<TradeConsumer>>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly PlutusDbContext _dbContext;
    private readonly Market _market;
    private readonly ConsumerDataLoaderOptions _options;

    public TradeConsumerTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();

        _market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "OSRS Market",
            new Taxes(null)
        );

        _options = new ConsumerDataLoaderOptions(
            IsEnabled: true,
            MarketMap: new Dictionary<Producer, string> { [Producer.Osrs] = _market.Id.Value }
        );
    }

    private TradeConsumer CreateConsumer()
    {
        return new(_logger, Options.Create(_options), _dbContext, _cache);
    }

    private TradeMessage CreateMessage(string symbolCode)
    {
        return new(
            Producer: Producer.Osrs,
            SymbolCode: symbolCode,
            SymbolSubcode: "p2p",
            SymbolName: "Test Item",
            Price: 1000m,
            Volume: 5m,
            Timestamp: DateTimeOffset.UtcNow,
            AdditionalFields: new AdditionalFields()
        );
    }

    [Fact]
    public async Task Handle_WhenSymbolDoesNotExist_ShouldCreateSymbolAndTrade()
    {
        // Arrange
        await _dbContext.SeedData(_market);
        var consumer = CreateConsumer();
        var message = CreateMessage("1234");

        // Act
        await consumer.Handle(message, CancellationToken.None);

        // Assert
        _dbContext.Symbols.Count().ShouldBe(1);
        _dbContext.Trades.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenSymbolAlreadyExists_ShouldUpdateSymbolAndCreateTrade()
    {
        // Arrange
        await _dbContext.SeedData(_market);

        var existing = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "1234",
            "p2p",
            "Old Name",
            _market.Id,
            new AdditionalFields()
        );

        await _dbContext.SeedData(existing);

        var consumer = CreateConsumer();
        var message = CreateMessage("1234");

        // Act
        await consumer.Handle(message, CancellationToken.None);

        // Assert
        _dbContext.Symbols.Count().ShouldBe(1);
        _dbContext.Trades.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenProducerNotInMarketMap_ShouldThrow()
    {
        // Arrange
        await _dbContext.SeedData(_market);
        var optionsWithNoMap = new ConsumerDataLoaderOptions(
            IsEnabled: true,
            MarketMap: new Dictionary<Producer, string>()
        );
        var consumer = new TradeConsumer(
            _logger,
            Options.Create(optionsWithNoMap),
            _dbContext,
            _cache
        );

        var message = CreateMessage("1234");

        // Act
        var act = async () => await consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }
}
