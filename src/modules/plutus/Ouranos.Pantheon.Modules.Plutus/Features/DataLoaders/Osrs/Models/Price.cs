namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;

public sealed record Price(
    int? AvgHighPrice,
    int HighPriceVolume,
    int? AvgLowPrice,
    int LowPriceVolume
);
