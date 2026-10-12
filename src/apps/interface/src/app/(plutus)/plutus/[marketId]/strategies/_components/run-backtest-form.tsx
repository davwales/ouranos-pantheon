"use client";

import { NumericInput } from "@/components/shared/numeric-input";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { plutusApi } from "@/lib/api/plutus";
import { useRouter } from "next/navigation";
import { useState } from "react";
import {
  DEFAULT_SLIPPAGE_MULTIPLIER,
  DEFAULT_VOLUME_PARTICIPATION_RATE,
} from "./execution-constants";

export function RunBacktestForm({
  strategyId,
  marketId,
  onDone,
  defaultStartDate,
  defaultEndDate,
  defaultBudget,
}: {
  strategyId: string;
  marketId: string;
  onDone: () => void;
  defaultStartDate?: string;
  defaultEndDate?: string;
  defaultBudget?: number;
}) {
  const router = useRouter();
  const [startDate, setStartDate] = useState(() => {
    if (defaultStartDate) return defaultStartDate;
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().split("T")[0];
  });
  const [endDate, setEndDate] = useState(() => {
    if (defaultEndDate) return defaultEndDate;
    const d = new Date();
    return d.toISOString().split("T")[0];
  });
  const [budget, setBudget] = useState(defaultBudget ?? 10000);
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
      await plutusApi.runBacktest(strategyId, {
        marketId,
        startDate,
        endDate,
        budget,
        volumeParticipationRate,
        slippageMultiplier,
      });
      onDone();
      router.push(`/plutus/${marketId}/strategies/${strategyId}/backtests`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to run backtest");
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
      <div className="space-y-1">
        <label htmlFor="backtest-start-date" className="text-sm font-medium block">
          Start Date
        </label>
        <Input
          id="backtest-start-date"
          type="date"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
        />
      </div>
      <div className="space-y-1">
        <label htmlFor="backtest-end-date" className="text-sm font-medium block">
          End Date
        </label>
        <Input
          id="backtest-end-date"
          type="date"
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
        />
      </div>
      {dateInvalid && (
        <p className="text-sm text-destructive">
          End date must be after start date
        </p>
      )}
      <NumericInput
        label="Budget"
        hint="Initial capital for the backtest"
        value={budget}
        onChange={(v) => setBudget(v ?? 0)}
        min={1}
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
        <div className="space-y-4 border rounded-lg p-3 bg-muted/30">
          <NumericInput
            label="Volume Participation Rate"
            hint="Max fraction of daily volume per trade (0-1)"
            value={volumeParticipationRate}
            onChange={(v) => setVolumeParticipationRate(v ?? DEFAULT_VOLUME_PARTICIPATION_RATE)}
            min={0.01}
            max={1}
            step={0.01}
          />
          <NumericInput
            label="Slippage Multiplier"
            hint="Square-root price impact coefficient (0 = fill at the daily average)"
            value={slippageMultiplier}
            onChange={(v) => setSlippageMultiplier(v ?? DEFAULT_SLIPPAGE_MULTIPLIER)}
            min={0}
            max={1}
            step={0.01}
          />
        </div>
      )}
      <Button
        className="w-full"
        onClick={handleSubmit}
        disabled={
          isSubmitting || !startDate || !endDate || budget < 1 || dateInvalid
        }
      >
        {isSubmitting ? "Starting..." : "Run Backtest"}
      </Button>
    </div>
  );
}
