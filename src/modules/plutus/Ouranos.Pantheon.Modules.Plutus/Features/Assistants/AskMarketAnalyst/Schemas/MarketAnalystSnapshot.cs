using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskMarketAnalyst.Schemas;

internal sealed record MarketAnalystSnapshot(
    GetMarketResponse Market,
    MarketAnalystContext Context,
    GetMarketOverviewResponse Overview,
    PagedResponse<GetMarketTradesResponse>? View,
    int TrendPoints,
    DateTimeOffset GeneratedAt
)
{
    public TimeFrame TimeFrame => Context.TimeFrame;
}
