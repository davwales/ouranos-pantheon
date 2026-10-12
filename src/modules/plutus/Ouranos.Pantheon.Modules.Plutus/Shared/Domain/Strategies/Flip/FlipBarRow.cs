namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

internal sealed record FlipBarRow(
    Guid SymbolId,
    DateTimeOffset Timestamp,
    decimal HighPrice,
    decimal HighVolume,
    decimal LowPrice,
    decimal LowVolume
);
