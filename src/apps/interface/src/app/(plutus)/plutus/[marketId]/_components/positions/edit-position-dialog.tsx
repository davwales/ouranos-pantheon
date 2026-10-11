"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { type Position } from "@/lib/api/plutus";
import { EditPositionForm } from "./edit-position-form";

export function EditPositionDialog({
  position,
  open,
  onOpenChange,
}: {
  position: Position;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  return (
    <ResponsiveDialog
      title="Edit Position"
      description="Update the position details below."
      open={open}
      onOpenChange={onOpenChange}
      trigger={null}
    >
      <EditPositionForm
        position={position}
        onDone={() => onOpenChange(false)}
      />
    </ResponsiveDialog>
  );
}
