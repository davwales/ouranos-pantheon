using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

public sealed record MarketOverviewBucket(
    Id<Market> MarketId,
    TimeFrame TimeFrame,
    DateTimeOffset BucketStart,
    decimal AveragePrice,
    decimal Volume,
    decimal TotalSpent,
    int NumTransactions
);
