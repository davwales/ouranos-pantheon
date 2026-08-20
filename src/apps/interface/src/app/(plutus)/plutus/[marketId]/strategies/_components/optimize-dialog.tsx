"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import { plutusApi } from "@/lib/api/plutus";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { OptimizeAdvancedOptions } from "./optimize-advanced-options";
import { OptimizeBasicOptions } from "./optimize-basic-options";

export function OptimizeDialog({
  strategyId,
  marketId,
  open,
  onOpenChange,
}: {
  strategyId: string;
  marketId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const [startDate, setStartDate] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().split("T")[0];
  });
  const [endDate, setEndDate] = useState(() => {
    const d = new Date();
    return d.toISOString().split("T")[0];
  });
  const [budget, setBudget] = useState(10000);
  const [generations, setGenerations] = useState(20);
  const [populationSize, setPopulationSize] = useState(20);
  const [sortinoWeight, setSortinoWeight] = useState(0.4);
  const [cagrWeight, setCagrWeight] = useState(0.3);
  const [drawdownWeight, setDrawdownWeight] = useState(0.5);
  const [turnoverWeight, setTurnoverWeight] = useState(0.1);
  const [l1RegularizationWeight, setL1RegularizationWeight] = useState(0.05);
  const [outSampleRatio, setOutSampleRatio] = useState(0.2);
  const [volumeParticipationRate, setVolumeParticipationRate] = useState(0.25);
  const [slippageMultiplier, setSlippageMultiplier] = useState(0.1);
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const dateInvalid = startDate > endDate;

  useEffect(() => {
    if (open) {
      const today = new Date().toISOString().split("T")[0];
      const monthAgo = new Date();
      monthAgo.setDate(monthAgo.getDate() - 30);
      setStartDate(monthAgo.toISOString().split("T")[0]);
      setEndDate(today);
      setBudget(10000);
      setGenerations(20);
      setPopulationSize(20);
      setSortinoWeight(0.4);
      setCagrWeight(0.3);
      setDrawdownWeight(0.5);
      setTurnoverWeight(0.1);
      setL1RegularizationWeight(0.05);
      setOutSampleRatio(0.2);
      setVolumeParticipationRate(0.25);
      setSlippageMultiplier(0.1);
      setShowAdvanced(false);
      setError(null);
    }
  }, [open]);

  const handleSubmit = async () => {
    setIsSubmitting(true);
    setError(null);
    try {
      await plutusApi.optimizeStrategy(strategyId, {
        marketId,
        startDate,
        endDate,
        budget,
        generations,
        populationSize,
        sortinoWeight,
        cagrWeight,
        drawdownWeight,
        turnoverWeight,
        l1RegularizationWeight,
        outSampleRatio,
        volumeParticipationRate,
        slippageMultiplier,
      });
      onOpenChange(false);
      router.push(`/plutus/${marketId}/strategies/${strategyId}/backtests`);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to optimize strategy",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ResponsiveDialog
      title="Optimize Strategy"
      description="Run a genetic algorithm to optimize strategy parameters."
      open={open}
      onOpenChange={onOpenChange}
      trigger={null}
    >
      <div className="space-y-4">
        {error && (
          <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
            {error}
          </div>
        )}
        <OptimizeBasicOptions
          startDate={startDate}
          onStartDateChange={setStartDate}
          endDate={endDate}
          onEndDateChange={setEndDate}
          budget={budget}
          onBudgetChange={setBudget}
          generations={generations}
          onGenerationsChange={setGenerations}
          populationSize={populationSize}
          onPopulationSizeChange={setPopulationSize}
          dateInvalid={dateInvalid}
        />
        <div className="pt-1">
          <Button
            type="button"
            variant="link"
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="text-sm text-muted-foreground hover:text-foreground underline"
          >
            {showAdvanced ? "Hide" : "Show"} Advanced Options
          </Button>
        </div>
        {showAdvanced && (
          <OptimizeAdvancedOptions
            sortinoWeight={sortinoWeight}
            onSortinoWeightChange={setSortinoWeight}
            cagrWeight={cagrWeight}
            onCagrWeightChange={setCagrWeight}
            drawdownWeight={drawdownWeight}
            onDrawdownWeightChange={setDrawdownWeight}
            turnoverWeight={turnoverWeight}
            onTurnoverWeightChange={setTurnoverWeight}
            l1RegularizationWeight={l1RegularizationWeight}
            onL1RegularizationWeightChange={setL1RegularizationWeight}
            outSampleRatio={outSampleRatio}
            onOutSampleRatioChange={setOutSampleRatio}
            volumeParticipationRate={volumeParticipationRate}
            onVolumeParticipationRateChange={setVolumeParticipationRate}
            slippageMultiplier={slippageMultiplier}
            onSlippageMultiplierChange={setSlippageMultiplier}
          />
        )}
        <Button
          className="w-full"
          onClick={handleSubmit}
          disabled={
            isSubmitting ||
            !startDate ||
            !endDate ||
            budget < 1 ||
            generations < 1 ||
            populationSize < 2 ||
            dateInvalid
          }
        >
          {isSubmitting ? "Starting..." : "Start Optimization"}
        </Button>
      </div>
    </ResponsiveDialog>
  );
}
