namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record BacktestResults(
    decimal TotalReturn,
    decimal TotalReturnPercent,
    decimal MaxDrawdown,
    decimal MaxDrawdownPercent,
    decimal WinRate,
    int TotalTrades,
    int WinningTrades,
    int LosingTrades,
    decimal SharpeRatio,
    decimal AverageTradeReturn,
    decimal BestTrade,
    decimal WorstTrade,
    decimal FinalBalance,
    List<BacktestPosition> Positions
)
{
    public BacktestResults() : this(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, [])
    {
    }
}