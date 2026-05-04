using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

/// <summary>
///     Represents a daily aggregation of trades for a symbol,
///     pre-computed by the database from the trades hypertable.
///     Used to build price buckets without loading raw trades.
/// </summary>
public sealed record DailyTradeAggregate(
    Id<Symbol> SymbolId,
    DateOnly Date,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume
);