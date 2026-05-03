namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;

public sealed record GetMarketOverviewResponse(
    decimal AveragePrice,
    decimal TotalSpent,
    decimal Volume,
    int NumTransactions,
    List<GetMarketOverviewBucketResponse> Trades
);
