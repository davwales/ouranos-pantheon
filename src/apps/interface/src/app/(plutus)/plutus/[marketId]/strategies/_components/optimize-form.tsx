"use client";

import { Button } from "@/components/ui/button";
import { plutusApi } from "@/lib/api/plutus";
import { useRouter } from "next/navigation";
import { useState } from "react";
import {
  DEFAULT_MIN_TRADES_PER_MONTH,
  DEFAULT_OPTIMIZE_DAYS,
  DEFAULT_OUT_SAMPLE_RATIO,
  DEFAULT_SLIPPAGE_MULTIPLIER,
  DEFAULT_VOLUME_PARTICIPATION_RATE,
} from "./execution-constants";
import { OptimizeAdvancedOptions } from "./optimize-advanced-options";
import { OptimizeBasicOptions } from "./optimize-basic-options";

export function OptimizeForm({
  strategyId,
  marketId,
  onDone,
}: {
  strategyId: string;
  marketId: string;
  onDone: () => void;
}) {
  const router = useRouter();
  const [startDate, setStartDate] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() - DEFAULT_OPTIMIZE_DAYS);
    return d.toISOString().split("T")[0];
  });
  const [endDate, setEndDate] = useState(() => {
    const d = new Date();
    return d.toISOString().split("T")[0];
  });
  const [budget, setBudget] = useState(10000);
  const [generations, setGenerations] = useState(30);
  const [populationSize, setPopulationSize] = useState(50);
  const [drawdownWeight, setDrawdownWeight] = useState(0.5);
  const [minTradesPerMonth, setMinTradesPerMonth] = useState(
    DEFAULT_MIN_TRADES_PER_MONTH,
  );
  const [outSampleRatio, setOutSampleRatio] = useState(
    DEFAULT_OUT_SAMPLE_RATIO,
  );
  const [volumeParticipationRate, setVolumeParticipationRate] = useState(
    DEFAULT_VOLUME_PARTICIPATION_RATE,
  );
  const [slippageMultiplier, setSlippageMultiplier] = useState(
    DEFAULT_SLIPPAGE_MULTIPLIER,
  );
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const dateInvalid = startDate > endDate;

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
        drawdownWeight,
        minTradesPerMonth,
        outSampleRatio,
        volumeParticipationRate,
        slippageMultiplier,
      });
      onDone();
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
          drawdownWeight={drawdownWeight}
          onDrawdownWeightChange={setDrawdownWeight}
          minTradesPerMonth={minTradesPerMonth}
          onMinTradesPerMonthChange={setMinTradesPerMonth}
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
  );
}
