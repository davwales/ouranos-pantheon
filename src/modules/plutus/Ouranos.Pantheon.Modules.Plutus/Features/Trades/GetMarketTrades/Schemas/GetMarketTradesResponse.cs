using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;

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
