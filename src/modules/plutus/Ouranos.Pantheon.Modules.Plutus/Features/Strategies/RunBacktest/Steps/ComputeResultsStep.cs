using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class ComputeResultsStep : IStep<BacktestPayload>
{
    private const decimal MaxRatio = 1_000_000_000m;

    public async Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        payload.Results = ComputeResults(
            payload.Parameters.Budget,
            payload.Portfolio.Balance,
            payload.Portfolio.MaxDrawdown,
            payload.Portfolio.PeakPortfolioValue,
            payload.Portfolio.ClosedPositions,
            payload.Portfolio.PortfolioValues,
            payload.Parameters.TotalDays
        );

        await Task.CompletedTask;
    }

    public static BacktestResults ComputeResults(
        decimal budget,
        decimal balance,
        decimal maxDrawdown,
        decimal peakPortfolioValue,
        List<BacktestPosition> closedPositions,
        List<decimal> portfolioValues,
        int totalDays
    )
    {
        var totalReturn = balance - budget;
        var totalReturnPercent = budget > 0 ? totalReturn / budget : 0;
        var winningTrades = closedPositions.Count(p => p.ProfitLoss > 0);
        var losingTrades = closedPositions.Count(p => p.ProfitLoss <= 0);
        var winRate =
            closedPositions.Count > 0 ? (decimal)winningTrades / closedPositions.Count : 0;
        var sharpeRatio = ComputeSharpeRatio(portfolioValues);
        var sortinoRatio = ComputeSortinoRatio(portfolioValues);
        var maxDrawdownAbsolute = maxDrawdown * peakPortfolioValue;
        var cagr = ComputeCagr(totalReturnPercent, totalDays);
        var calmarRatio = maxDrawdown != 0 ? Math.Min(cagr / maxDrawdown, MaxRatio) : 0;
        var grossProfit = closedPositions.Where(p => p.ProfitLoss > 0).Sum(p => p.ProfitLoss);
        var grossLoss = Math.Abs(
            closedPositions.Where(p => p.ProfitLoss < 0).Sum(p => p.ProfitLoss)
        );
        var profitFactor =
            grossLoss > 0 ? grossProfit / grossLoss
            : grossProfit > 0 ? MaxRatio
            : 0;
        var avgWin = winningTrades > 0 ? grossProfit / winningTrades : 0;
        var avgLoss = losingTrades > 0 ? grossLoss / losingTrades : 0;
        var expectancy = (winRate * avgWin) - ((1 - winRate) * avgLoss);

        return new BacktestResults
        {
            TotalReturn = totalReturn,
            TotalReturnPercent = totalReturnPercent,
            MaxDrawdown = maxDrawdownAbsolute,
            MaxDrawdownPercent = maxDrawdown,
            WinRate = winRate,
            TotalTrades = closedPositions.Count,
            WinningTrades = winningTrades,
            LosingTrades = losingTrades,
            SharpeRatio = sharpeRatio,
            SortinoRatio = sortinoRatio,
            CalmarRatio = calmarRatio,
            Cagr = cagr,
            ProfitFactor = profitFactor,
            Expectancy = expectancy,
            AverageTradeReturn =
                closedPositions.Count > 0 ? closedPositions.Average(p => p.ReturnPercent) : 0,
            BestTrade = closedPositions.Count > 0 ? closedPositions.Max(p => p.ReturnPercent) : 0,
            WorstTrade = closedPositions.Count > 0 ? closedPositions.Min(p => p.ReturnPercent) : 0,
            FinalBalance = balance,
            TurnoverRate = totalDays > 0 ? (decimal)closedPositions.Count / totalDays : 0m,
            IsValidated = false,
            OutSampleResults = null,
            OptimizedInputWeights = null,
            OptimizedThresholds = null,
            OptimizedConfiguration = null,
        };
    }

    public static decimal ComputeSharpeRatio(List<decimal> portfolioValues)
    {
        if (portfolioValues.Count <= 2)
        {
            return 0m;
        }

        var returns = new List<decimal>();
        for (var i = 1; i < portfolioValues.Count; i++)
        {
            if (portfolioValues[i - 1] != 0)
            {
                returns.Add((portfolioValues[i] - portfolioValues[i - 1]) / portfolioValues[i - 1]);
            }
        }

        if (returns.Count < 2)
        {
            return 0m;
        }

        var avgReturn = returns.Average();
        var sampleVariance =
            returns.Sum(r => (r - avgReturn) * (r - avgReturn)) / (returns.Count - 1);
        var stdDev = (decimal)Math.Sqrt((double)sampleVariance);
        return stdDev > 0 ? Math.Min(avgReturn / stdDev * (decimal)Math.Sqrt(365), MaxRatio) : 0;
    }

    public static decimal ComputeSortinoRatio(List<decimal> portfolioValues)
    {
        if (portfolioValues.Count <= 2)
        {
            return 0m;
        }

        var returns = new List<decimal>();
        for (var i = 1; i < portfolioValues.Count; i++)
        {
            if (portfolioValues[i - 1] != 0)
            {
                returns.Add((portfolioValues[i] - portfolioValues[i - 1]) / portfolioValues[i - 1]);
            }
        }

        if (returns.Count < 2)
        {
            return 0m;
        }

        var avgReturn = returns.Average();
        var downsideReturns = returns.Where(r => r < 0).ToList();

        if (downsideReturns.Count == 0)
        {
            return avgReturn > 0 ? MaxRatio : 0;
        }

        var downsideVariance = downsideReturns.Sum(r => r * r) / downsideReturns.Count;
        var downsideDeviation = (decimal)Math.Sqrt((double)downsideVariance);
        return downsideDeviation > 0
            ? Math.Min(avgReturn / downsideDeviation * (decimal)Math.Sqrt(365), MaxRatio)
            : 0;
    }

    public static decimal ComputeCagr(decimal totalReturnPercent, int totalDays)
    {
        if (totalDays <= 0 || totalReturnPercent <= -1m)
        {
            return 0m;
        }

        var finalMultiplier = 1m + totalReturnPercent;
        if (finalMultiplier <= 0)
        {
            return -1m;
        }

        var years = totalDays / 365.0;
        var cagrDouble = Math.Pow((double)finalMultiplier, 1.0 / years) - 1.0;
        if (double.IsNaN(cagrDouble) || cagrDouble > (double)MaxRatio)
        {
            return MaxRatio;
        }
        var cagr = (decimal)cagrDouble;
        return Math.Min(cagr, MaxRatio);
    }
}
