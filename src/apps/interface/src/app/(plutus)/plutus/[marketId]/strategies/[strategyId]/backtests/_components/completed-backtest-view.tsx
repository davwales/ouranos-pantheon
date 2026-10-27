import {
  type BacktestDetail,
  type BacktestResults,
  type StrategyDetail,
} from "@/lib/api/plutus";
import { BacktestMetricsGrid } from "./backtest-metrics-grid";
import { BacktestPositionsTable } from "./backtest-positions-table";
import { BacktestResultHeader } from "./backtest-result-header";
import { BacktestStatisticsGrid } from "./backtest-statistics-grid";
import { OptimizedConfigurationCard } from "./optimized-configuration-card";

type CompletedBacktestViewProps = {
  backtest: BacktestDetail;
  results: BacktestResults;
  marketId: string;
  strategy: StrategyDetail | undefined;
  isApplyingConfig: boolean;
  onRefresh: () => void;
  onRunAgain: () => void;
  onApplyToStrategy?: () => void;
};

/** Results dashboard for a completed backtest or optimization. */
export function CompletedBacktestView({
  backtest,
  results,
  marketId,
  strategy,
  isApplyingConfig,
  onRefresh,
  onRunAgain,
  onApplyToStrategy,
}: CompletedBacktestViewProps) {
  return (
    <>
      <BacktestResultHeader
        backtest={backtest}
        marketId={marketId}
        strategy={strategy}
        onRefresh={onRefresh}
        onRunAgain={onRunAgain}
      />

      <BacktestMetricsGrid results={results} />

      <BacktestStatisticsGrid results={results} />

      {results.optimizedConfiguration != null && (
        <OptimizedConfigurationCard
          results={results}
          isApplying={isApplyingConfig}
          onApplyToStrategy={onApplyToStrategy}
        />
      )}

      {backtest.positions && backtest.positions.length > 0 && (
        <BacktestPositionsTable positions={backtest.positions} />
      )}
    </>
  );
}
