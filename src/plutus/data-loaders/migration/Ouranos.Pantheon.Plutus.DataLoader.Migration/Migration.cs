using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration;

public sealed class Migration : IMigration
{
    private readonly ILogger<Migration> _logger;
    private readonly IMongoDatabase _mongoDatabase;

    public Migration(
        ILogger<Migration> logger,
        IMongoDatabaseManager mongoDatabaseManager
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mongoDatabaseManager);

        _logger = logger;
        _mongoDatabase = mongoDatabaseManager.GetDatabase<Migration>();
    }

    public async Task Migrate(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting time series migration...");
        cancellationToken.ThrowIfCancellationRequested();

        var legacyTradesCollection = _mongoDatabase.GetCollection<Trade>("trades_bak");
        var timeSeriesTradesCollection = _mongoDatabase.GetCollection<Trade>("trades_test");

        var tradeCursor = await legacyTradesCollection
            .Find(Builders<Trade>.Filter.Empty)
            .Sort(Builders<Trade>.Sort.Ascending(t => t.CreatedAt))
            .ToCursorAsync(cancellationToken);

        long numProcessed = 0;
        var start = DateTimeOffset.UtcNow;

        while (await tradeCursor.MoveNextAsync(cancellationToken))
        {
            var trades = tradeCursor.Current.ToList();

            if (trades.Count == 0)
            {
                continue;
            }

            await timeSeriesTradesCollection.InsertManyAsync(trades, null, cancellationToken);

            numProcessed += trades.Count;
            var duration = DateTimeOffset.UtcNow.Subtract(start);
            _logger.LogInformation(
                "Processed '{count}' trades after a total of '{seconds}' seconds. Throughput: '{throughput}' trades/s.",
                numProcessed,
                duration.TotalSeconds,
                duration.TotalSeconds > 0 ? numProcessed / duration.TotalSeconds : 0
            );

            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.LogInformation("Completed time series migration.");
    }
}