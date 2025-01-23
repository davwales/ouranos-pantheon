using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.GetTrades;

public sealed class GetTradesAction : IGetTradesAction
{
    private readonly ILogger<GetTradesAction> _logger;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly AggregateOptions _options;

    private readonly string _talosTradesCollectionName;

    public GetTradesAction(
        ILogger<GetTradesAction> logger,
        IMongoDatabaseManager mongoDatabaseManager,
        IConfiguration configuration
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mongoDatabaseManager);
        Guard.Against.Null(configuration);

        _logger = logger;
        _mongoDatabase = mongoDatabaseManager.GetDatabase<TalosTrade>();

        _options = new AggregateOptions
        {
            BatchSize = configuration.GetValue<int?>("Ouranos:BatchSize")
        };

        _talosTradesCollectionName = configuration
            .GetSection("Ouranos:Mongo:TalosTradesCollectionName")
            .Get<string>() ?? throw new InvalidOperationException("Invalid talos-trades collection configuration.");
    }

    public async Task<IAsyncCursor<TalosTrade>> GetTradesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to get trades cursor.");
        cancellationToken.ThrowIfCancellationRequested();

        PipelineDefinition<TalosTrade, TalosTrade> pipeline = new[]
        {
            new BsonDocument("$lookup",
                new BsonDocument
                {
                    { "from", "trademigrations" },
                    { "localField", "_id" },
                    { "foreignField", "_id" },
                    { "as", "migration" }
                }
            ),
            new BsonDocument("$match",
                new BsonDocument("migration",
                    new BsonDocument("$size", 0)
                )
            ),
            new BsonDocument("$project",
                new BsonDocument("migration", 0)
            )
        };

        var cursor = await _mongoDatabase
            .GetCollection<TalosTrade>(_talosTradesCollectionName)
            .AggregateAsync(pipeline, _options, cancellationToken);

        _logger.LogDebug("Successfully retrieved trades cursor.");
        return cursor;
    }
}