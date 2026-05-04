using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

/// <summary>
///     Represents the closing price for a symbol on a given date,
///     pre-aggregated from the trades table by the database.
/// </summary>
public sealed record DailyPrice(
    Id<Symbol> SymbolId,
    DateOnly Date,
    decimal ClosePrice
);