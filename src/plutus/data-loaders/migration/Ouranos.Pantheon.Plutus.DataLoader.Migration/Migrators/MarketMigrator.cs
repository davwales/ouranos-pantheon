using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Migrators;

public class MarketMigrator
{
    private readonly ILogger<MarketMigrator> _logger;
    private readonly IRepository<Market> _marketRepository;
    private readonly IMongoDatabase _mongoDatabase;

    public MarketMigrator(
        ILogger<MarketMigrator> logger,
        IRepository<Market> marketRepository,
        IMongoDatabaseManager mongoDatabaseManager
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(marketRepository);
        Guard.Against.Null(mongoDatabaseManager);

        _logger = logger;
        _marketRepository = marketRepository;
        _mongoDatabase = mongoDatabaseManager.GetDatabase<Migration>();
    }

    public async Task<Dictionary<Id<Market>, Id<Market>>> Migrate(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting market migration.");
        cancellationToken.ThrowIfCancellationRequested();

        var existingMarkets = await _marketRepository.ReadAll(cancellationToken);
        var existingMarketIds = existingMarkets.Select(m => m.Id).ToList();

        if (existingMarketIds.Count > 0)
        {
            _logger.LogDebug("Found {marketCount} markets that have been previously migrated.", existingMarketIds.Count);
        }

        var marketCollection = _mongoDatabase.GetCollection<Market>("markets");
        var legacyMarkets = await marketCollection.Find(Builders<Market>.Filter.Empty).ToListAsync(cancellationToken);

        // Markets were migrated as static data previously.
        var marketMap = new Dictionary<Id<Market>, Id<Market>>
        {
            {
                new Id<Market>("65678cc3a579e897dee76113"), new Id<Market>("d71d7207-e30b-404f-8797-0148ad88cf9e")
            }, // OSRS
            {
                new Id<Market>("65565d09d4f9e2fd3aefe674"), new Id<Market>("411b954f-5834-462e-9887-26d3ad76c924")
            }, // FFXIV
            {
                new Id<Market>("65650c286ae59a057449b04c"), new Id<Market>("daebf0a1-b54d-44f4-9c21-6654c505169a")
            } // Stock Market
        };

        List<Market> markets = [];
        foreach (var (legacyId, newId) in marketMap)
        {
            if (existingMarketIds.Contains(newId))
            {
                continue;
            }

            var legacyMarket = legacyMarkets.FirstOrDefault(m => m.Id == legacyId)
                ?? throw new KeyNotFoundException($"Legacy market with ID {legacyId} not found.");

            markets.Add(Market.Create(
                newId,
                legacyMarket.Name,
                legacyMarket.Taxes,
                legacyMarket.IsForecastingEnabled,
                legacyMarket.Description,
                legacyMarket.Icon
            ));
        }

        await _marketRepository.CreateMany(markets, cancellationToken);
        await _marketRepository.SaveChanges(cancellationToken);
        _logger.LogInformation("Successfully migrated {migratedCount} markets.", markets.Count);

        return marketMap;
    }
}
