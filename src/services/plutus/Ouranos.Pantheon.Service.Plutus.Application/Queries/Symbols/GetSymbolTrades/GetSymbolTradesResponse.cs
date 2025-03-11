namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Symbols.GetSymbolTrades;

public sealed record GetSymbolTradesResponse(
    decimal MinPrice,
    decimal MaxPrice,
    decimal AveragePrice,
    decimal TotalSpent,
    decimal Volume,
    int NumTransactions,
    List<GetSymbolTradeBucketsResponse> Trades
);