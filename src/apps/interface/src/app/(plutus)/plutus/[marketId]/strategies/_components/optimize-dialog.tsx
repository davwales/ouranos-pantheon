"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { OptimizeForm } from "./optimize-form";

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
        onDone={() => onOpenChange(false)}
      />
    </ResponsiveDialog>
  );
}
