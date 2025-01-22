using MongoDB.Bson.Serialization.Attributes;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

public sealed record TalosAdditionalFields(
    decimal? Limit,
    [property: BsonElement("highalch")] int? HighAlch,
    [property: BsonElement("lowalch")] int? LowAlch
);