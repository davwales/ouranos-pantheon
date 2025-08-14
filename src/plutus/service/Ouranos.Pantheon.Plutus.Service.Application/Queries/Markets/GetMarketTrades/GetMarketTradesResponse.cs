using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketTrades;

public sealed record GetMarketTradesResponse(
    Symbol Symbol,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume,
    int NumTransactions,
    decimal Margin,
    decimal AveragePrice,
    decimal Roi,
    decimal TotalGain,
    decimal Limit
);