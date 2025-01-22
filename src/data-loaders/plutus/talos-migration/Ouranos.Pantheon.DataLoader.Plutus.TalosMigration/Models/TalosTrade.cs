using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

public sealed record TalosTrade(
    ObjectId Id,
    DateTimeOffset Date,
    decimal Price,
    decimal Volume,
    [property: BsonElement("metadata")] TalosTradeMetaData? MetaData
);