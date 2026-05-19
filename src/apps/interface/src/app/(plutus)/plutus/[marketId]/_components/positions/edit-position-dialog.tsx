"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { NumericInput } from "@/components/shared/numeric-input";
import { type Position, plutusApi } from "@/lib/api/plutus";
import { useEffect, useState } from "react";

export function EditPositionDialog({
  position,
  open,
  onOpenChange,
}: {
  position: Position;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const [cost, setCost] = useState<number | null>(position.cost);
  const [quantity, setQuantity] = useState<number | null>(position.quantity);
  const [notes, setNotes] = useState(position.notes ?? "");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setCost(position.cost);
      setQuantity(position.quantity);
      setNotes(position.notes ?? "");
    }
  }, [open, position]);

  const handleSubmit = async () => {
    if (cost == null || quantity == null) return;

    setIsSubmitting(true);
    setError(null);
    try {
      await plutusApi.updatePosition(position.id, {
        cost,
        quantity,
        notes: notes.trim() || null,
      });
      onOpenChange(false);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update position",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ResponsiveDialog
      title="Edit Position"
      description="Update the position details below."
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
          <NumericInput
            label="Cost"
            value={cost}
            onChange={setCost}
            min={0}
            step={0.01}
          />
        </div>
        <div className="space-y-1">
          <NumericInput
            label="Quantity"
            value={quantity}
            onChange={setQuantity}
            min={1}
            step={1}
          />
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium block">Notes</label>
          <Input value={notes} onChange={(e) => setNotes(e.target.value)} />
        </div>
        <Button
          className="w-full"
          onClick={handleSubmit}
          disabled={
            isSubmitting ||
            cost == null ||
            cost < 0 ||
            quantity == null ||
            quantity < 1
          }
        >
          {isSubmitting ? "Saving..." : "Save Changes"}
        </Button>
      </div>
    </ResponsiveDialog>
  );
}
