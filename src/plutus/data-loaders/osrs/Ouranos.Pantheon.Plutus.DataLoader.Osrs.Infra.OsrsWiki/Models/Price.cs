namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Infra.OsrsWiki.Models;

public sealed record Price(
    int? AvgHighPrice,
    int HighPriceVolume,
    int? AvgLowPrice,
    int LowPriceVolume
);