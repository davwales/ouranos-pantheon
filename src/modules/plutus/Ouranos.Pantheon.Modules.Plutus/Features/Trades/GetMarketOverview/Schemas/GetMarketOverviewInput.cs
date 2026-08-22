using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;

public sealed record GetMarketOverviewInput(
    Id<Market> MarketId,
    TimeFrame TimeFrame = TimeFrame.OneHour,
    int NumBuckets = 100
);
