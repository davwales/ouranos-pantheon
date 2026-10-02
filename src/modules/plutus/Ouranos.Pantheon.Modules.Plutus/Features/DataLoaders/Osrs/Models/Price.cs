namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;

public sealed record Price(
    long? AvgHighPrice,
    long HighPriceVolume,
    long? AvgLowPrice,
    long LowPriceVolume
);
