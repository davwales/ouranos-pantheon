namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate.Schemas;

/// <summary>
///     Row type for raw SQL query results from TimescaleDB.
///     Maps the server-side time-bucket aggregation of <c>plutus.trades</c>
///     so the 24h window is never materialized into memory.
/// </summary>
public sealed record SymbolBucketRow(
    Guid SymbolId,
    DateTimeOffset BucketStart,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Volume
);
