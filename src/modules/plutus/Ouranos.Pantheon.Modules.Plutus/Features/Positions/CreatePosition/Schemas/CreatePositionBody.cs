using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition.Schemas;

public sealed record CreatePositionBody(
    PositionSide Side,
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    decimal Cost,
    decimal Quantity,
    Id<Strategy>? StrategyId = null,
    string? Notes = null
);