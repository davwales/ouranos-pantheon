"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { CreatePositionForm } from "./create-position-form";

export function CreatePositionDialog({
  marketId,
  open,
  onOpenChange,
  linkedBuyPositionId,
  defaultSymbolId,
  defaultSymbolName,
}: {
  marketId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  linkedBuyPositionId?: string | null;
  defaultSymbolId?: string;
  defaultSymbolName?: string;
}) {
  const title = linkedBuyPositionId
    ? "Create Sell Position"
    : "Create Position";

  return (
    <ResponsiveDialog
      title={title}
      description="Enter the position details below."
      open={open}
      onOpenChange={onOpenChange}
      trigger={null}
    >
      <CreatePositionForm
        marketId={marketId}
        onDone={() => onOpenChange(false)}
        linkedBuyPositionId={linkedBuyPositionId}
        defaultSymbolId={defaultSymbolId}
        defaultSymbolName={defaultSymbolName}
      />
    </ResponsiveDialog>
  );
}
