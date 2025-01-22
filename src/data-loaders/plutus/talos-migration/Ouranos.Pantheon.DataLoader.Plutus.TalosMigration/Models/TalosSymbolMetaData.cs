using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

public sealed record TalosSymbolMetaData(
    string Code,
    string? Subcode,
    string Name,
    [property: BsonElement("market_id")] ObjectId MarketId,
    [property: BsonElement("additional_fields")]
    TalosAdditionalFields? AdditionalFields
);