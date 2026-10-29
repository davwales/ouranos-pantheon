"use client";

import { ConfirmationButton } from "@/components/shared/confirmation-button";
import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import { RefreshCw } from "lucide-react";

type GroupDetailHeaderProps = {
  isProcessing: boolean;
  canSave: boolean;
  onSave: () => void;
  onDelete: () => void;
};

export function GroupDetailHeader({
  isProcessing,
  canSave,
  onSave,
  onDelete,
}: GroupDetailHeaderProps) {
  return (
    <div className="flex items-center justify-between">
      <Typography variant="h2" className="border-b-0">
        Group Details
      </Typography>
      <div className="flex items-center gap-2">
        {isProcessing && <RefreshCw className="animate-spin" />}
        <Button onClick={onSave} disabled={isProcessing || !canSave}>
          Save
        </Button>
        <ConfirmationButton
          title="Delete Group"
          description="Are you sure you want to delete this group? This action cannot be undone."
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
