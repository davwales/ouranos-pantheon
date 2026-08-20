"use client";

import { StrategyDetailSkeleton } from "@/app/(plutus)/plutus/[marketId]/strategies/_components/strategy-detail-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";
import { useApi } from "@/hooks/use-api";
import { type StrategyDetail, plutusApi } from "@/lib/api/plutus";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { OptimizeDialog } from "../_components/optimize-dialog";
import { RunBacktestDialog } from "../_components/run-backtest-dialog";
import { StrategyConfigForm } from "../_components/strategy-config-form";
import { StrategyHeader } from "../_components/strategy-header";
import { StrategyReadOnlyView } from "../_components/strategy-read-only-view";

export default function StrategyDetailPage() {
  const { marketId, strategyId } = useParams<{
    marketId: string;
    strategyId: string;
  }>();
  const router = useRouter();

  const [strategy, reexecute] = useApi<StrategyDetail>(
    () => plutusApi.getStrategy(strategyId),
    [strategyId],
  );

  const [isEditing, setIsEditing] = useState(false);
  const [toggling, setToggling] = useState(false);
  const [runBacktestOpen, setRunBacktestOpen] = useState(false);
  const [optimizeOpen, setOptimizeOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const data = strategy.data;

  const handleToggleActive = async () => {
    if (!data) return;
    setToggling(true);
    setError(null);
    try {
      await plutusApi.setStrategyActive(strategyId, !data.isActive);
      reexecute();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to toggle active status",
      );
    } finally {
      setToggling(false);
    }
  };

  const handleDelete = async () => {
    setError(null);
    try {
      await plutusApi.deleteStrategy(strategyId);
      router.replace(`/plutus/${marketId}/strategies`);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete strategy",
      );
    }
  };

  const handleEditSuccess = () => {
    reexecute();
    setIsEditing(false);
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
  };

  if (strategy.status === "error" && !data) {
    return (
      <NotFoundCard
        title="Strategy not found"
        backHref={`/plutus/${marketId}/strategies`}
        backLabel="Back to Strategies"
      />
    );
  }

  if (!data) {
    return <StrategyDetailSkeleton />;
  }

  return (
    <div className="space-y-6">
      {error && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}
      <StrategyHeader
        data={data}
        marketId={marketId}
        strategyId={strategyId}
        isEditing={isEditing}
        toggling={toggling}
        onEdit={() => setIsEditing(true)}
        onToggleActive={handleToggleActive}
        onDelete={handleDelete}
        onRunBacktest={() => setRunBacktestOpen(true)}
        onOptimize={() => setOptimizeOpen(true)}
      />

      {isEditing ? (
        <StrategyConfigForm
          mode="edit"
          marketId={data.marketId}
          strategyId={data.id}
          initialStrategy={data}
          onSuccess={handleEditSuccess}
          onCancel={handleCancelEdit}
        />
      ) : (
        <StrategyReadOnlyView data={data} />
      )}

      <RunBacktestDialog
        strategyId={strategyId}
        marketId={marketId}
        open={runBacktestOpen}
        onOpenChange={setRunBacktestOpen}
      />

      <OptimizeDialog
        strategyId={strategyId}
        marketId={marketId}
        open={optimizeOpen}
        onOpenChange={setOptimizeOpen}
      />
    </div>
  );
}
