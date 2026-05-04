using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

/// <summary>
///     Internal DTO for EF Core LINQ GroupBy projection.
///     EF Core requires concrete types for GroupBy Select projections.
/// </summary>
internal sealed record DailyTradeAggregateDto(
    Id<Symbol> SymbolId,
    DateTimeOffset Date,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume
);