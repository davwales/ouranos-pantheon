import { BacktestResults } from "@/lib/api/plutus";
import { ResultCard } from "./result-card";

export function BacktestMetricsGrid({ results }: { results: BacktestResults }) {
  return (
    <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <ResultCard
        label="Total Return %"
        value={`${(results.totalReturnPercent * 100).toFixed(2)}%`}
        color={
          results.totalReturnPercent >= 0
            ? "text-green-600 dark:text-green-400"
            : "text-red-600 dark:text-red-400"
        }
      />
      <ResultCard label="Sharpe Ratio" value={results.sharpeRatio.toFixed(2)} />
      <ResultCard
        label="Win Rate"
        value={`${(results.winRate * 100).toFixed(1)}%`}
      />
      <ResultCard
        label="Max Drawdown %"
        value={`${(results.maxDrawdownPercent * 100).toFixed(2)}%`}
        color="text-red-600 dark:text-red-400"
      />
    </div>
  );
}
