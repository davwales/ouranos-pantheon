"use client";

import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use-interval";
import { type BacktestDetail, type StrategyDetail, plutusApi } from "@/lib/api/plutus";
import { BacktestDetailSkeleton } from "@/app/(plutus)/plutus/[marketId]/backtests/_components/backtest-detail-skeleton";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useCallback, useState } from "react";
import { RunBacktestDialog } from "../../strategies/_components/run-backtest-dialog";
import { BacktestMetricsGrid } from "../_components/backtest-metrics-grid";
import { BacktestPositionsTable } from "../_components/backtest-positions-table";
import { BacktestResultHeader } from "../_components/backtest-result-header";
import { BacktestStatisticsGrid } from "../_components/backtest-statistics-grid";
import { CancelledBacktestView } from "../_components/cancelled-backtest-view";
import { FailedBacktestView } from "../_components/failed-backtest-view";
import { InProgressBacktestView } from "../_components/in-progress-backtest-view";
import { OptimizedConfigurationCard } from "../_components/optimized-configuration-card";
import { NotFoundCard } from "@/components/shared/not-found-card";

export default function BacktestDetailPage() {
  const { marketId, backtestId } = useParams<{
    marketId: string;
    backtestId: string;
  }>();
  const router = useRouter();

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

  const [isCancelling, setIsCancelling] = useState(false);
  const [isRestarting, setIsRestarting] = useState(false);
  const [isApplyingConfig, setIsApplyingConfig] = useState(false);
  const [runAgainOpen, setRunAgainOpen] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const handleCancel = useCallback(async () => {
    setIsCancelling(true);
    setActionError(null);
    try {
      await plutusApi.cancelBacktest(backtestId);
      reexecute();
    } catch (err) {
      setActionError(
        err instanceof Error ? err.message : "Failed to cancel backtest",
      );
    } finally {
      setIsCancelling(false);
    }
  }, [backtestId, reexecute]);

  const handleRestart = useCallback(async () => {
    setIsRestarting(true);
    setActionError(null);
    try {
      await plutusApi.restartBacktest(backtestId);
      reexecute();
    } catch (err) {
      setActionError(
        err instanceof Error ? err.message : "Failed to restart backtest",
      );
    } finally {
      setIsRestarting(false);
    }
  }, [backtestId, reexecute]);

  const handleApplyToStrategy = useCallback(async () => {
    if (
      !backtest?.strategyId ||
      !backtest.results ||
      !strategy
    ) {
      return;
    }
    setIsApplyingConfig(true);
    setActionError(null);
    try {
      await plutusApi.updateStrategy(backtest.strategyId, {
        name: strategy.name,
        description: strategy.description ?? null,
        tradingConfiguration:
          backtest.results.optimizedConfiguration ??
          strategy.tradingConfiguration,
        inputWeights:
          backtest.results.optimizedInputWeights ?? strategy.inputWeights,
        thresholds:
          backtest.results.optimizedThresholds ?? strategy.thresholds,
      });
      router.push(`/plutus/${marketId}/strategies/${backtest.strategyId}`);
    } catch (err) {
      setActionError(
        err instanceof Error ? err.message : "Failed to apply configuration",
      );
    } finally {
      setIsApplyingConfig(false);
    }
  }, [
    backtest?.strategyId,
    backtest?.results,
    strategy,
    marketId,
    router,
  ]);

  if (backtestState.status === "error") {
    return <NotFoundCard title="Backtest not found" message="This backtest doesn\u0027t exist or has been removed." backHref={`/plutus/${marketId}/strategies`} backLabel="Back to Strategies" />;
  }

  if (!backtest) {
    return <BacktestDetailSkeleton />;
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

      {actionError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {actionError}
        </div>
      )}

      {backtest.status === "Pending" && (
        <InProgressBacktestView
          status={backtest.status}
          progressPercent={backtest.progressPercent}
          progressMessage={
            backtest.progressMessage ?? "Waiting to be queued for execution..."
          }
          onCancel={handleCancel}
          isCancelling={isCancelling}
        />
      )}

      {backtest.status === "Running" && (
        <InProgressBacktestView
          status={backtest.status}
          progressPercent={backtest.progressPercent}
          progressMessage={backtest.progressMessage ?? "Running simulation..."}
          onCancel={handleCancel}
          isCancelling={isCancelling}
        />
      )}

      {backtest.status === "Failed" && (
        <FailedBacktestView
          errorMessage={backtest.errorMessage}
          onRestart={handleRestart}
          isRestarting={isRestarting}
        />
      )}

      {backtest.status === "Cancelled" && (
        <CancelledBacktestView
          errorMessage={backtest.errorMessage}
          onRestart={handleRestart}
          isRestarting={isRestarting}
        />
      )}

      {backtest.status === "Completed" && results && (
        <>
          <BacktestResultHeader
            backtest={backtest}
            marketId={marketId}
            strategy={strategy}
            onRefresh={reexecute}
            onRunAgain={() => setRunAgainOpen(true)}
          />

          <BacktestMetricsGrid results={results} />

          <BacktestStatisticsGrid results={results} />

          {(results.optimizedConfiguration != null ||
            results.optimizedInputWeights != null ||
            results.optimizedThresholds != null) && (
            <OptimizedConfigurationCard
              results={results}
              isApplying={isApplyingConfig}
              onApplyToStrategy={strategy ? handleApplyToStrategy : undefined}
            />
          )}

          {backtest.positions && backtest.positions.length > 0 && (
            <BacktestPositionsTable positions={backtest.positions} />
          )}
        </>
      )}

      <RunBacktestDialog
        strategyId={backtest.strategyId}
        marketId={marketId}
        open={runAgainOpen}
        onOpenChange={setRunAgainOpen}
        defaultStartDate={backtest.startDate.split("T")[0]}
        defaultEndDate={backtest.endDate.split("T")[0]}
        defaultBudget={backtest.budget}
      />
    </div>
  );
}
