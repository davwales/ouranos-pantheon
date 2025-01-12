namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki.Models;

public sealed record Price(
    int? AvgHighPrice,
    int HighPriceVolume,
    int? AvgLowPrice,
    int LowPriceVolume
);