"use client";

import { ResponsiveDialog } from "@/app/components/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { plutusApi } from "@/lib/api/plutus";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { NumberInput } from "./number-input";

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
  const [generations, setGenerations] = useState(100);
  const [populationSize, setPopulationSize] = useState(50);
  const [sharpeRatioWeight, setSharpeRatioWeight] = useState(0.5);
  const [totalReturnWeight, setTotalReturnWeight] = useState(0.3);
  const [maxDrawdownWeight, setMaxDrawdownWeight] = useState(-0.2);
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
        sharpeRatioWeight,
        totalReturnWeight,
        maxDrawdownWeight,
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
        <div className="space-y-1">
          <label className="text-sm font-medium block">Start Date</label>
          <Input
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
          />
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium block">End Date</label>
          <Input
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
        <NumberInput
          label="Budget"
          hint="Initial capital for optimization"
          value={budget}
          onChange={(v) => setBudget(v ?? 0)}
          min={1}
        />
        <NumberInput
          label="Generations"
          hint="Number of optimization generations"
          value={generations}
          onChange={(v) => setGenerations(v ?? 1)}
          min={1}
          max={500}
          step={1}
        />
        <NumberInput
          label="Population Size"
          hint="Population per generation"
          value={populationSize}
          onChange={(v) => setPopulationSize(v ?? 2)}
          min={2}
          max={200}
          step={1}
        />
        <div className="pt-1">
          <button
            type="button"
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="text-sm text-muted-foreground hover:text-foreground underline"
          >
            {showAdvanced ? "Hide" : "Show"} Advanced Options
          </button>
        </div>
        {showAdvanced && (
          <div className="space-y-4 border rounded-lg p-3 bg-muted/30">
            <NumberInput
              label="Sharpe Ratio Weight"
              hint="Weight for risk-adjusted return"
              value={sharpeRatioWeight}
              onChange={(v) => setSharpeRatioWeight(v ?? 0)}
              step={0.1}
              min={-10}
              max={10}
            />
            <NumberInput
              label="Total Return Weight"
              hint="Weight for total return"
              value={totalReturnWeight}
              onChange={(v) => setTotalReturnWeight(v ?? 0)}
              step={0.1}
              min={-10}
              max={10}
            />
            <NumberInput
              label="Max Drawdown Weight"
              hint="Negative weight penalizes drawdown"
              value={maxDrawdownWeight}
              onChange={(v) => setMaxDrawdownWeight(v ?? 0)}
              step={0.1}
              min={-10}
              max={10}
            />
          </div>
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
