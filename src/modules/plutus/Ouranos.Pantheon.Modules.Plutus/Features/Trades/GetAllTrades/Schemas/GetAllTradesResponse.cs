using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;

public sealed record GetAllTradesResponse(
    Id<Trade> Id,
    Id<Symbol> SymbolId,
    Id<Market> MarketId,
    string SymbolName,
    string SymbolCode,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp
);
