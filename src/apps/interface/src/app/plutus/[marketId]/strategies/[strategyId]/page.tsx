"use client";

import { StrategyDetailSkeleton } from "@/app/components/skeletons/strategy-detail-skeleton";
import { Typography } from "@/app/components/typography";
import { useApi } from "@/hooks/use-api";
import {
  StrategyConfigBundle,
  StrategyDetail,
  plutusApi,
} from "@/lib/api/plutus";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { OptimizeDialog } from "../components/optimize-dialog";
import { RunBacktestDialog } from "../components/run-backtest-dialog";
import { StrategyEditForm } from "../components/strategy-edit-form";
import { StrategyHeader } from "../components/strategy-header";
import { StrategyReadOnlyView } from "../components/strategy-read-only-view";

function detailToBundle(detail: StrategyDetail): StrategyConfigBundle {
  return {
    tradingConfiguration: detail.tradingConfiguration,
    signalWeightedConfig: detail.signalWeightedConfig,
    forecastMomentumConfig: detail.forecastMomentumConfig,
    meanReversionConfig: detail.meanReversionConfig,
    recipeArbitrageConfig: detail.recipeArbitrageConfig,
    components: detail.components,
  };
}

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
  const [isSaving, setIsSaving] = useState(false);
  const [toggling, setToggling] = useState(false);
  const [runBacktestOpen, setRunBacktestOpen] = useState(false);
  const [optimizeOpen, setOptimizeOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [bundle, setBundle] = useState<StrategyConfigBundle>({
    tradingConfiguration: {
      maxPositions: 10,
      maxPositionPercent: 0.2,
      holdPeriodDays: 7,
    },
  });

  const data = strategy.data;

  useEffect(() => {
    if (data) {
      setName(data.name);
      setDescription(data.description ?? "");
      setBundle(detailToBundle(data));
    }
  }, [data]);

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

  const handleSave = async () => {
    if (!data) return;
    setIsSaving(true);
    setError(null);
    try {
      await plutusApi.updateStrategy(strategyId, {
        name: name.trim(),
        description: description.trim() || null,
        ...bundle,
      });
      reexecute();
      setIsEditing(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save strategy");
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    if (data) {
      setName(data.name);
      setDescription(data.description ?? "");
      setBundle(detailToBundle(data));
    }
    setIsEditing(false);
  };

  if (strategy.status === "error" && !data) {
    return <Typography variant="lead">Error loading strategy</Typography>;
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
        isSaving={isSaving}
        toggling={toggling}
        editedName={name}
        onEdit={() => setIsEditing(true)}
        onCancel={handleCancel}
        onSave={handleSave}
        onToggleActive={handleToggleActive}
        onDelete={handleDelete}
        onRunBacktest={() => setRunBacktestOpen(true)}
        onOptimize={() => setOptimizeOpen(true)}
      />

      {isEditing ? (
        <StrategyEditForm
          data={data}
          name={name}
          description={description}
          bundle={bundle}
          onNameChange={setName}
          onDescriptionChange={setDescription}
          onBundleChange={setBundle}
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
