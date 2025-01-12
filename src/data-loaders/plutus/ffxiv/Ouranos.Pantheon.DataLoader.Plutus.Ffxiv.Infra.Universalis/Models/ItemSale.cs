using MongoDB.Bson.Serialization.Attributes;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis.Models;

public sealed record ItemSale(
    bool Hq,
    int PricePerUnit,
    int Quantity,
    int Timestamp,
    bool OnMannequin,
    string? WorldName,
    [property: BsonElement("worldID")] int? WorldId,
    string BuyerName,
    int Total
);