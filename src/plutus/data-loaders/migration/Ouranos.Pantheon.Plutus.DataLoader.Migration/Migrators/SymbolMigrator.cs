using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Plutus.DataLoader.Migration.Models;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Migrators;

public class SymbolMigrator
{
    private readonly ILogger<SymbolMigrator> _logger;
    private readonly IRepository<Market> _marketRepository;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IRepository<Symbol> _symbolRepository;

    public SymbolMigrator(
        ILogger<SymbolMigrator> logger,
        IMongoDatabaseManager mongoDatabaseManager,
        IRepository<Market> marketRepository,
        IRepository<Symbol> symbolRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mongoDatabaseManager);
        Guard.Against.Null(marketRepository);
        Guard.Against.Null(symbolRepository);

        _logger = logger;
        _mongoDatabase = mongoDatabaseManager.GetDatabase<Migration>();
        _marketRepository = marketRepository;
        _symbolRepository = symbolRepository;
    }

    public async Task<Dictionary<Id<Symbol>, Id<Symbol>>> MigrateAsync(
        IReadOnlyDictionary<Id<Market>, Id<Market>> marketIdMap,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Starting symbol migration...");

        var symbolMapCollection = _mongoDatabase.GetCollection<SymbolIdMapping>("migration_symbol_map");
        var existingMappings = await symbolMapCollection.Find(Builders<SymbolIdMapping>.Filter.Empty)
            .ToListAsync(cancellationToken);

        var symbolIdMap = existingMappings.ToDictionary(
            m => new Id<Symbol>(m.Id),
            m => new Id<Symbol>(m.NewId)
        );
        var migratedLegacySymbolIds = new HashSet<Id<Symbol>>(symbolIdMap.Keys);

        _logger.LogInformation("Found {count} already migrated symbols.", migratedLegacySymbolIds.Count);

        var legacySymbolCollection = _mongoDatabase.GetCollection<Symbol>("symbols");
        var allLegacySymbols = await legacySymbolCollection.Find(Builders<Symbol>.Filter.Empty)
            .ToListAsync(cancellationToken);
        var symbolsToMigrate = allLegacySymbols.Where(s => !migratedLegacySymbolIds.Contains(s.Id)).ToList();

        if (symbolsToMigrate.Count != 0)
        {
            var markets = await _marketRepository.ReadAll(cancellationToken);
            var marketDictionary = markets.ToDictionary(m => m.Id, m => m);
            var newMappings = new List<SymbolIdMapping>(symbolsToMigrate.Count);

            foreach (var legacySymbol in symbolsToMigrate)
            {
                var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
                symbolIdMap[legacySymbol.Id] = symbolId;
                var marketId = marketIdMap[legacySymbol.MarketId];

                var symbol = Symbol.Create(
                    symbolId,
                    legacySymbol.Code,
                    legacySymbol.Subcode,
                    legacySymbol.Name,
                    marketDictionary[marketId],
                    legacySymbol.AdditionalFields
                );

                await _symbolRepository.Create(symbol, cancellationToken);

                newMappings.Add(
                    new SymbolIdMapping(
                        legacySymbol.Id.ToString(),
                        symbolId.ToString()
                    )
                );
            }

            await _symbolRepository.SaveChanges(cancellationToken);

            if (newMappings.Count != 0)
            {
                await symbolMapCollection.InsertManyAsync(newMappings, cancellationToken: cancellationToken);
            }
        }

        _logger.LogInformation("Completed symbol migration, migrated '{count}' symbols", symbolsToMigrate.Count);
        return symbolIdMap;
    }
}