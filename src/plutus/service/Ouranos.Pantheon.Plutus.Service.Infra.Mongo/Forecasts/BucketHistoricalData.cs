using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Models.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Mongo.Forecasts;

public sealed class BucketHistoricalData : IBucketHistoricalData
{
    private readonly ILogger<BucketHistoricalData> _logger;

    public BucketHistoricalData(ILogger<BucketHistoricalData> logger)
    {
        Guard.Against.Null(logger);
        _logger = logger;
    }

    public IQueryable<ForecastBucketDto> ApplyBucketing(IQueryable<Trade> query)
    {
        _logger.LogTrace("Attempting to apply bucketing to historical forecasting data.");

        if (query is not IMongoQueryable<Trade> mongoQuery)
        {
            throw new InvalidOperationException("Cannot apply forecasting bucketing to query.");
        }

        var bucketedQuery = mongoQuery
            .AppendStage<Trade, ForecastBucketDto>(
                new BsonDocument(
                    "$group",
                    new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                { "symbolId", "$metadata.symbolId" },
                                {
                                    "bucket", new BsonDocument(
                                        "$dateTrunc",
                                        new BsonDocument
                                        {
                                            { "date", "$createdAt" },
                                            { "unit", "day" }
                                        }
                                    )
                                }
                            }
                        },
                        { "minPrice", new BsonDocument("$min", "$price") },
                        { "maxPrice", new BsonDocument("$max", "$price") },
                        { "volume", new BsonDocument("$sum", "$volume") },
                        {
                            "totalSpent", new BsonDocument(
                                "$sum",
                                new BsonDocument(
                                    "$multiply",
                                    new BsonArray
                                    {
                                        "$price",
                                        "$volume"
                                    }
                                )
                            )
                        }
                    }
                )
            );

        _logger.LogDebug("Successfully applied bucketing to historical forecasting data.");
        return bucketedQuery;
    }
}