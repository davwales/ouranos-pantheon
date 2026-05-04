"use client";

import { ResponsiveDialog } from "@/app/components/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { plutusApi } from "@/lib/api/plutus";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { NumberInput } from "./number-input";

export function RunBacktestDialog({
  strategyId,
  marketId,
  open,
  onOpenChange,
  defaultStartDate,
  defaultEndDate,
  defaultBudget,
}: {
  strategyId: string;
  marketId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
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
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const dateInvalid = startDate > endDate;

  useEffect(() => {
    if (open) {
      if (defaultStartDate) setStartDate(defaultStartDate);
      if (defaultEndDate) setEndDate(defaultEndDate);
      if (defaultBudget != null) setBudget(defaultBudget);
    }
  }, [open, defaultStartDate, defaultEndDate, defaultBudget]);

  const handleSubmit = async () => {
    setIsSubmitting(true);
    setError(null);
    try {
      await plutusApi.runBacktest(strategyId, {
        marketId,
        startDate,
        endDate,
        budget,
      });
      onOpenChange(false);
      router.push(`/plutus/${marketId}/strategies/${strategyId}/backtests`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to run backtest");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ResponsiveDialog
      title="Run Backtest"
      description="Configure the backtest parameters and start the simulation."
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
          hint="Initial capital for the backtest"
          value={budget}
          onChange={(v) => setBudget(v ?? 0)}
          min={1}
        />
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
    </ResponsiveDialog>
  );
}
