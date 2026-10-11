"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { RunBacktestForm } from "./run-backtest-form";

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
  return (
    <ResponsiveDialog
      title="Run Backtest"
      description="Configure the backtest parameters and start the simulation."
      open={open}
      onOpenChange={onOpenChange}
      trigger={null}
    >
      <RunBacktestForm
        strategyId={strategyId}
        marketId={marketId}
        onDone={() => onOpenChange(false)}
        defaultStartDate={defaultStartDate}
        defaultEndDate={defaultEndDate}
        defaultBudget={defaultBudget}
      />
    </ResponsiveDialog>
  );
}
