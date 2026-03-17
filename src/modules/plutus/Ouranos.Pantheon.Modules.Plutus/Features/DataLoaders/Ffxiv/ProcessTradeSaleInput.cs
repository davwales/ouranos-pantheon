namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;

public sealed record ProcessTradeSaleInput(
    bool IsHighQuality,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp
);
