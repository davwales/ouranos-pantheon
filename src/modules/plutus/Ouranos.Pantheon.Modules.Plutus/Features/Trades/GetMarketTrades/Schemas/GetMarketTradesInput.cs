using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;

public sealed record GetMarketTradesInput(
    Id<Market> MarketId,
    double? Seconds = null,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
);
