namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;

public sealed record PriceResponse(
    Dictionary<string, Price> Data,
    int Timestamp
);
