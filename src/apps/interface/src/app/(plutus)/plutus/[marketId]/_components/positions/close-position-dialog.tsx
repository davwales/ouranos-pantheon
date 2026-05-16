"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import {
  type Position,
  type PositionStatus,
  plutusApi,
} from "@/lib/api/plutus";
import { useState } from "react";
import { positionSideLabels, positionStatusLabels } from "./position-constants";

export function ClosePositionDialog({
  position,
  open,
  onOpenChange,
  onClosed,
}: {
  position: Position;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onClosed?: () => void;
}) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleClose = async (closeStatus: PositionStatus) => {
    setIsSubmitting(true);
    setError(null);
    try {
      await plutusApi.closePosition(position.id, closeStatus);
      onOpenChange(false);
      if (closeStatus === "Bought" && onClosed) {
        onClosed();
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to close position");
    } finally {
      setIsSubmitting(false);
    }
  };

  const closeOptions: PositionStatus[] =
    position.side === "Buy" ? ["DidNotBuy", "Bought"] : ["DidNotSell", "Sold"];

  return (
    <ResponsiveDialog
      title="Close Position"
      description="Select the outcome for this position."
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
        <div className="rounded-lg border p-3 bg-muted/30 space-y-1">
          <div className="text-sm">
            <span className="text-muted-foreground">Symbol:</span>{" "}
            {position.symbolName}
          </div>
          <div className="text-sm">
            <span className="text-muted-foreground">Side:</span>{" "}
            {positionSideLabels[position.side]}
          </div>
          <div className="text-sm">
            <span className="text-muted-foreground">Cost:</span> {position.cost}
          </div>
          <div className="text-sm">
            <span className="text-muted-foreground">Quantity:</span>{" "}
            {position.quantity}
          </div>
          <div className="text-sm">
            <span className="text-muted-foreground">Current Status:</span>{" "}
            {positionStatusLabels[position.status]}
          </div>
        </div>
        <div className="flex gap-2">
          {closeOptions.map((status) => (
            <Button
              key={status}
              variant={
                status === "Bought" || status === "Sold" ? "default" : "outline"
              }
              onClick={() => handleClose(status)}
              disabled={isSubmitting}
              className="flex-1"
            >
              {positionStatusLabels[status]}
            </Button>
          ))}
        </div>
      </div>
    </ResponsiveDialog>
  );
}
