namespace Talos.Olympus.Service.Plutus.Application.Queries.Trades.GetSymbolTrades;

public sealed record GetSymbolTradesResponse(
    decimal MinPrice,
    decimal MaxPrice,
    decimal AveragePrice,
    decimal TotalSpent,
    decimal Margin,
    decimal TotalGain,
    decimal AverageGain,
    int NumTransactions,
    decimal Tax,
    List<GetSymbolTradeBucketsResponse> Trades
);