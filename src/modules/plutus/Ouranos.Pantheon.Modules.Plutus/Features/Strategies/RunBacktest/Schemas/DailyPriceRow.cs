namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

/// <summary>
///     Internal row type for raw SQL query results from TimescaleDB.
///     Uses primitive types compatible with EF Core <c>SqlQueryRaw</c> mapping.
/// </summary>
internal sealed record DailyPriceRow(Guid SymbolId, DateTimeOffset Date, decimal ClosePrice);
