using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Markets.GetMarketTrades;

public sealed record GetMarketTradesResponse(
    Symbol Symbol,
    decimal TotalSpent,
    decimal MinPrice,
    decimal MaxPrice,
    decimal TotalVolume,
    int NumTransactions,
    decimal Limit,
    decimal Tax
)
{
    public decimal Margin => MaxPrice - MinPrice - Tax;
    public decimal AveragePrice => TotalSpent / TotalVolume;
    public decimal Roi => (MaxPrice - MinPrice - Tax) / MinPrice;
    public decimal TotalGain => (MaxPrice - MinPrice - Tax) * (TotalVolume > Limit ? Limit : TotalVolume);
}