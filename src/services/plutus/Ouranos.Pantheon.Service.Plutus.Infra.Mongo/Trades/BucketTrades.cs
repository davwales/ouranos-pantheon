using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Application.Models.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Mongo.Trades;

public sealed class BucketTrades : IBucketTrades
{
    private readonly ILogger<BucketTrades> _logger;

    public BucketTrades(ILogger<BucketTrades> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public IQueryable<BucketDto> GetBucketedTradesQuery(
        IQueryable<Trade> query,
        int numBuckets,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to bucket trades for a Mongo query.");
        cancellationToken.ThrowIfCancellationRequested();

        if (query is not IMongoQueryable<Trade> mongoQuery)
            throw new InvalidOperationException("The input query does not support Mongo bucketing!");

        var bucketQuery = mongoQuery.AppendStage<Trade, BucketDto>(new BsonDocument("$bucketAuto", new BsonDocument
        {
            { "groupBy", "$createdAt" },
            { "buckets", numBuckets },
            {
                "output", new BsonDocument
                {
                    { "symbolId", new BsonDocument("$last", "$metadata.symbol._id") },
                    { "date", new BsonDocument("$last", "$createdAt") },
                    {
                        "totalSpent", new BsonDocument
                        {
                            {
                                "$sum", new BsonDocument
                                {
                                    { "$multiply", new BsonArray { "$price", "$volume" } }
                                }
                            }
                        }
                    },
                    { "volume", new BsonDocument("$sum", "$volume") },
                    { "minPrice", new BsonDocument("$min", "$price") },
                    { "maxPrice", new BsonDocument("$max", "$price") },
                    { "numTransactions", new BsonDocument("$sum", 1) }
                }
            }
        }));

        _logger.LogDebug("Successfully bucketed trades for a Monog query.");
        return bucketQuery;
    }
}