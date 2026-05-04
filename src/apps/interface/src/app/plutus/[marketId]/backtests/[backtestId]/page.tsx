"use client";

import { Typography } from "@/app/components/typography";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use_interval";
import { BacktestDetail, StrategyDetail, plutusApi } from "@/lib/api/plutus";
import { ArrowLeft, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useCallback } from "react";
import { BacktestMetricsGrid } from "../components/backtest-metrics-grid";
import { BacktestPositionsTable } from "../components/backtest-positions-table";
import { BacktestResultHeader } from "../components/backtest-result-header";
import { BacktestStatisticsGrid } from "../components/backtest-statistics-grid";
import { FailedBacktestView } from "../components/failed-backtest-view";
import { InProgressBacktestView } from "../components/in-progress-backtest-view";

export default function BacktestDetailPage() {
  const { marketId, backtestId } = useParams<{
    marketId: string;
    backtestId: string;
  }>();

  const [backtestState, reexecute] = useApi<BacktestDetail>(
    () => plutusApi.getBacktest(backtestId),
    [backtestId],
  );

  const backtest = backtestState.data;

  const isPolling =
    backtest?.status === "Pending" || backtest?.status === "Running";

  const pollBacktest = useCallback(() => {
    reexecute();
  }, [reexecute]);

  useInterval(pollBacktest, isPolling ? 3000 : null);

  const [strategyState] = useApi<StrategyDetail | null>(
    () =>
      backtest?.strategyId
        ? plutusApi.getStrategy(backtest.strategyId)
        : Promise.resolve(null),
    [backtest?.strategyId],
  );

  const strategy = strategyState.data ?? undefined;

  if (backtestState.status === "error") {
    return (
      <div className="space-y-4">
        <Link
          href={`/plutus/${marketId}/strategies/${backtest?.strategyId ?? ""}/backtests`}
          className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="w-4 h-4 mr-1" />
          Back to Backtests
        </Link>
        <Typography variant="lead">Error loading backtest</Typography>
      </div>
    );
  }

  if (!backtest) {
    return (
      <div className="flex items-center justify-center py-8">
        <RefreshCw className="animate-spin" />
      </div>
    );
  }

  const results = backtest.results;

  return (
    <div className="space-y-6">
      <Link
        href={`/plutus/${marketId}/strategies/${backtest.strategyId}/backtests`}
        className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="w-4 h-4 mr-1" />
        Back to Backtests
      </Link>

      {backtest.status === "Pending" && (
        <InProgressBacktestView
          status={backtest.status}
          progressPercent={backtest.progressPercent}
          progressMessage={
            backtest.progressMessage ?? "Waiting to be queued for execution..."
          }
        />
      )}

      {backtest.status === "Running" && (
        <InProgressBacktestView
          status={backtest.status}
          progressPercent={backtest.progressPercent}
          progressMessage={backtest.progressMessage ?? "Running simulation..."}
        />
      )}

      {backtest.status === "Failed" && (
        <FailedBacktestView errorMessage={backtest.errorMessage} />
      )}

      {backtest.status === "Completed" && results && (
        <>
          <BacktestResultHeader
            backtest={backtest}
            marketId={marketId}
            strategy={strategy}
            onRefresh={reexecute}
          />

          <BacktestMetricsGrid results={results} />

          <BacktestStatisticsGrid results={results} />

          {results.positions && results.positions.length > 0 && (
            <BacktestPositionsTable positions={results.positions} />
          )}
        </>
      )}
    </div>
  );
}
