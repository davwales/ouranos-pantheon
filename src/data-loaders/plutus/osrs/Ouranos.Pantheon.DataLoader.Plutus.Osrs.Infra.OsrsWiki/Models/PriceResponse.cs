namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Dtos;

public sealed record PriceResponse(
    Dictionary<string, Price> Data,
    int Timestamp
);