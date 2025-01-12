namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki.Models;

public sealed record PriceResponse(
    Dictionary<string, Price> Data,
    int Timestamp
);