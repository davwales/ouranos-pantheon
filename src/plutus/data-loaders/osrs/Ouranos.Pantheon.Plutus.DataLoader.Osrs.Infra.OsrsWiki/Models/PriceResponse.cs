namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Infra.OsrsWiki.Models;

public sealed record PriceResponse(
    Dictionary<string, Price> Data,
    int Timestamp
);