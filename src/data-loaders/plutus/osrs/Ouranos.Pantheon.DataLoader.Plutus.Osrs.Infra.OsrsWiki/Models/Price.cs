namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Dtos;

public sealed record Price(
    int? AvgHighPrice,
    int HighPriceVolume,
    int? AvgLowPrice,
    int LowPriceVolume
);