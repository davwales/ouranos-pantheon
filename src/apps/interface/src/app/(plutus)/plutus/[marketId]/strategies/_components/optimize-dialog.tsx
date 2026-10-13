"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { type StrategyConfiguration } from "@/lib/api/plutus";
import { OptimizeForm } from "./optimize-form";

export function OptimizeDialog({
  strategyId,
  marketId,
  configuration,
  open,
  onOpenChange,
  defaultBudget,
}: {
  strategyId: string;
  marketId: string;
  configuration: StrategyConfiguration;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  defaultBudget?: number;
}) {
  return (
    <ResponsiveDialog
      title="Optimize Strategy"
      description="Run a genetic algorithm to optimize strategy parameters."
      open={open}
      onOpenChange={onOpenChange}
      trigger={null}
    >
      <OptimizeForm
        strategyId={strategyId}
        marketId={marketId}
        configuration={configuration}
        onDone={() => onOpenChange(false)}
        defaultBudget={defaultBudget}
      />
    </ResponsiveDialog>
  );
}
