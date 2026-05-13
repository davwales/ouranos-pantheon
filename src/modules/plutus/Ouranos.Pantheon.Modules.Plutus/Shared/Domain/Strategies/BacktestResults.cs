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
    decimal? SortinoRatio,
    decimal? CalmarRatio,
    decimal? Cagr,
    decimal? ProfitFactor,
    decimal? Expectancy,
    decimal AverageTradeReturn,
    decimal BestTrade,
    decimal WorstTrade,
    decimal FinalBalance,
    SignalWeightedConfig? OptimizedSignalWeightedConfig,
    ForecastMomentumConfig? OptimizedForecastMomentumConfig,
    MeanReversionConfig? OptimizedMeanReversionConfig,
    RecipeArbitrageConfig? OptimizedRecipeArbitrageConfig,
    TradingConfiguration? OptimizedConfiguration
)
{
    public BacktestResults()
        : this(
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            null,
            null,
            null,
            null,
            null,
            0,
            0,
            0,
            0,
            null,
            null,
            null,
            null,
            null
        ) { }
}
