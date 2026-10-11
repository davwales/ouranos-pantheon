"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { ImportRecipeForm } from "./import-recipe-form";

export type ImportRecipeDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function ImportRecipeDialog({
  open,
  onOpenChange,
}: ImportRecipeDialogProps) {
  return (
    <ResponsiveDialog
      title="Import from Link"
      description="Paste a recipe URL and we'll fetch and parse it for you."
      open={open}
      onOpenChange={onOpenChange}
      trigger={null}
    >
      <ImportRecipeForm onDone={() => onOpenChange(false)} />
    </ResponsiveDialog>
  );
}
