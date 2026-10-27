"use client";

import TimeFrameSelection from "@/app/(plutus)/plutus/components/time-frame-selection";
import { ConfirmationButton } from "@/components/shared/confirmation-button";
import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import { RefreshCw } from "lucide-react";

type RecipeDetailHeaderProps = {
  isProcessing: boolean;
  onSave: () => void;
  onDelete: () => void;
};

export function RecipeDetailHeader({
  isProcessing,
  onSave,
  onDelete,
}: RecipeDetailHeaderProps) {
  return (
    <div className="flex items-center justify-between">
      <Typography variant="h2" className="border-b-0">
        Recipe Details
      </Typography>

      <div className="flex items-center gap-2">
        <TimeFrameSelection />
        {isProcessing && <RefreshCw className="animate-spin" />}
        <Button onClick={onSave} disabled={isProcessing}>
          Save
        </Button>
        <ConfirmationButton
          title="Delete Recipe"
          description="Are you sure you want to delete this recipe? This action cannot be undone."
          onConfirm={onDelete}
          disabled={isProcessing}
          variant="destructive"
        >
          Delete
        </ConfirmationButton>
      </div>
    </div>
  );
}
