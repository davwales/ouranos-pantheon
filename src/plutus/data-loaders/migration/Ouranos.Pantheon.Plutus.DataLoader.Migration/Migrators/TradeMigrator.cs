using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Plutus.DataLoader.Migration.Models;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;
using Ouranos.Pantheon.Plutus.Service.Infra.Postgres;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Migrators;

public class TradeMigrator
{
    private readonly ILogger<TradeMigrator> _logger;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly int _batchSize = 10000;

    public TradeMigrator(
        ILogger<TradeMigrator> logger,
        IMongoDatabaseManager mongoDatabaseManager,
        IDbContextFactory<PlutusDbContext> dbContextFactory
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mongoDatabaseManager);
        Guard.Against.Null(dbContextFactory);

        _logger = logger;
        _mongoDatabase = mongoDatabaseManager.GetDatabase<Migration>();
        _dbContextFactory = dbContextFactory;
    }

    public async Task MigrateAsync(
        IReadOnlyDictionary<Id<Symbol>, Id<Symbol>> symbolIdMap,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Starting time series migration...");
        cancellationToken.ThrowIfCancellationRequested();

        var migrationState = await GetOrCreateMigrationStateAsync(cancellationToken);
        using var tradeCursor = await GetLegacyTradeCursorAsync(migrationState, cancellationToken);

        long numProcessed = 0;
        var start = DateTimeOffset.UtcNow;

        while (await tradeCursor.MoveNextAsync(cancellationToken))
        {
            var batch = tradeCursor.Current.ToList();
            if (batch.Count == 0)
            {
                continue;
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var legacySymbolIds = batch.Select(b => b.Metadata.SymbolId).ToList();
            var migratedSymbolIds = legacySymbolIds.Select(id => symbolIdMap[id]).ToList();
            var symbols = await dbContext
                .Symbols.Where(s => migratedSymbolIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
            var symbolDictionary = symbols.ToDictionary(s => s.Id, s => s);

            var newTrades = TransformLegacyTrades(batch, symbolIdMap, symbolDictionary);
            if (newTrades.Count == 0)
            {
                continue;
            }

            await dbContext.Trades.AddRangeAsync(newTrades, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            numProcessed += newTrades.Count;
            DateTimeOffset? lastBatchCreatedAt = batch.Last().CreatedAt;

            LogProgress(numProcessed, start);

            migrationState.LastMigratedTradeCreatedAt = lastBatchCreatedAt;
            await UpdateMigrationStateAsync(migrationState, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.LogInformation("Completed time series migration.");
    }

    private List<Trade> TransformLegacyTrades(
        IReadOnlyList<LegacyTrade> legacyTrades,
        IReadOnlyDictionary<Id<Symbol>, Id<Symbol>> symbolIdMap,
        IReadOnlyDictionary<Id<Symbol>, Symbol> symbolDictionary
    )
    {
        var trades = new List<Trade>(legacyTrades.Count);

        foreach (var legacyTrade in legacyTrades)
        {
            if (!symbolIdMap.TryGetValue(legacyTrade.Metadata.SymbolId, out var symbolId))
            {
                throw new InvalidOperationException(
                    $"Could not find mapping for legacy symbol '{legacyTrade.Metadata.SymbolId}'."
                );
            }

            if (!symbolDictionary.TryGetValue(symbolId, out var symbol))
            {
                throw new InvalidOperationException($"Could not find symbol entity '{symbolId}'.");
            }

            var tradeId = new Id<Trade>(Guid.NewGuid().ToString());
            trades.Add(
                Trade.Create(
                    tradeId,
                    symbol,
                    legacyTrade.Price,
                    legacyTrade.Volume,
                    legacyTrade.CreatedAt
                )
            );
        }

        return trades;
    }

    private async Task<IAsyncCursor<LegacyTrade>> GetLegacyTradeCursorAsync(
        MigrationState migrationState,
        CancellationToken cancellationToken
    )
    {
        var legacyTradesCollection = _mongoDatabase.GetCollection<LegacyTrade>("trades");

        var filter = Builders<LegacyTrade>.Filter.Empty;
        if (migrationState.LastMigratedTradeCreatedAt.HasValue)
        {
            _logger.LogInformation(
                "Resuming trade migration from {timestamp}",
                migrationState.LastMigratedTradeCreatedAt.Value
            );
            filter = Builders<LegacyTrade>.Filter.Gt(
                t => t.CreatedAt,
                migrationState.LastMigratedTradeCreatedAt.Value
            );
        }

        return await legacyTradesCollection
            .Find(filter, options: new FindOptions() { BatchSize = _batchSize })
            .Sort(Builders<LegacyTrade>.Sort.Ascending(t => t.CreatedAt))
            .ToCursorAsync(cancellationToken);
    }

    private async Task<MigrationState> GetOrCreateMigrationStateAsync(
        CancellationToken cancellationToken
    )
    {
        var migrationStateCollection = _mongoDatabase.GetCollection<MigrationState>("migration_state");
        var migrationState =
            await migrationStateCollection
                .Find(s => s.Id == "trades")
                .FirstOrDefaultAsync(cancellationToken)
            ?? new MigrationState("trades");
        return migrationState;
    }

    private async Task UpdateMigrationStateAsync(
        MigrationState migrationState,
        CancellationToken cancellationToken
    )
    {
        var migrationStateCollection = _mongoDatabase.GetCollection<MigrationState>("migration_state");
        await migrationStateCollection.ReplaceOneAsync(
            s => s.Id == "trades",
            migrationState,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken
        );
    }

    private void LogProgress(long numProcessed, DateTimeOffset start)
    {
        var duration = DateTimeOffset.UtcNow.Subtract(start);
        _logger.LogInformation(
            "Processed '{count}' trades after a total of '{seconds}' seconds. Throughput: '{throughput}' trades/s.",
            numProcessed,
            duration.TotalSeconds,
            duration.TotalSeconds > 0 ? numProcessed / duration.TotalSeconds : 0
        );
    }
}
