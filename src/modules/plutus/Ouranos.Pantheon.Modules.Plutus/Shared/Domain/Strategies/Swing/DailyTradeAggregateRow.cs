namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>
///     Internal row type for raw SQL query results from TimescaleDB.
///     Uses primitive types compatible with EF Core <c>SqlQueryRaw</c> mapping.
/// </summary>
internal sealed record DailyTradeAggregateRow(
    Guid SymbolId,
    DateTimeOffset Date,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume
);
