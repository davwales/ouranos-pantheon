using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Symbols;

namespace Talos.Olympus.Service.Plutus.Application.Queries.Trades.GetMarketTrades;

public sealed record GetMarketTradesResponse(
    Id<Symbol> SymbolId,
    string SymbolName,
    string SymbolCode,
    string? SymbolSubcode,
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