namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;

public sealed record GetSymbolTradesResponse(
    decimal MinPrice,
    decimal MaxPrice,
    decimal AveragePrice,
    decimal TotalSpent,
    decimal Volume,
    int NumTransactions,
    IEnumerable<GetSymbolTradeBucketsResponse> Trades
);
