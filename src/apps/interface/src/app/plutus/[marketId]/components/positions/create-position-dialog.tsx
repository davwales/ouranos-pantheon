"use client";

import { ResponsiveDialog } from "@/app/components/responsive-dialog/responsive-dialog";
import { SymbolSelect } from "@/app/plutus/components/symbol-select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { type PositionSide, plutusApi } from "@/lib/api/plutus";
import { useEffect, useState } from "react";

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
  const [selectedSymbolId, setSelectedSymbolId] = useState<string | null>(
    defaultSymbolId ?? null,
  );
  const [selectedSymbolName, setSelectedSymbolName] = useState<string | null>(
    defaultSymbolName ?? null,
  );
  const [side, setSide] = useState<PositionSide>(
    linkedBuyPositionId ? "Sell" : "Buy",
  );
  const [cost, setCost] = useState<number | null>(null);
  const [quantity, setQuantity] = useState<number | null>(null);
  const [notes, setNotes] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isSymbolReadOnly = !!defaultSymbolId;

  useEffect(() => {
    if (open) {
      setSelectedSymbolId(defaultSymbolId ?? null);
      setSelectedSymbolName(defaultSymbolName ?? null);
      setSide(linkedBuyPositionId ? "Sell" : "Buy");
      setCost(null);
      setQuantity(null);
      setNotes("");
      setError(null);
    }
  }, [open, defaultSymbolId, defaultSymbolName, linkedBuyPositionId]);

  const handleSubmit = async () => {
    if (cost == null || quantity == null || !selectedSymbolId) return;

    setIsSubmitting(true);
    setError(null);
    try {
      const result = await plutusApi.createPosition({
        side,
        marketId,
        symbolId: selectedSymbolId,
        cost,
        quantity,
        strategyId: null,
        notes: notes.trim() || null,
      });

      if (linkedBuyPositionId && result.id) {
        await plutusApi.linkPosition(result.id, linkedBuyPositionId);
      }

      onOpenChange(false);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create position",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

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
      <div className="space-y-4">
        {error && (
          <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
            {error}
          </div>
        )}
        <div className="space-y-1">
          <label className="text-sm font-medium block">Symbol</label>
          {isSymbolReadOnly ? (
            <Input value={selectedSymbolName ?? ""} disabled />
          ) : (
            <SymbolSelect
              marketId={marketId}
              value={
                selectedSymbolId && selectedSymbolName
                  ? { id: selectedSymbolId, name: selectedSymbolName }
                  : null
              }
              onChange={(symbol) => {
                setSelectedSymbolId(symbol?.id ?? null);
                setSelectedSymbolName(symbol?.name ?? null);
              }}
            />
          )}
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium block">Side</label>
          <div className="flex gap-2">
            {(["Buy", "Sell"] as PositionSide[]).map((s) => (
              <Button
                key={s}
                type="button"
                variant={side === s ? "default" : "outline"}
                onClick={() => setSide(s)}
                disabled={!!linkedBuyPositionId}
                className="flex-1"
              >
                {s}
              </Button>
            ))}
          </div>
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium block">Cost</label>
          <Input
            type="number"
            value={cost ?? ""}
            onChange={(e) => {
              const v =
                e.target.value === "" ? null : parseFloat(e.target.value);
              setCost(v == null || isNaN(v) ? null : v);
            }}
            min={0}
            step={0.01}
          />
        </div>
        <div className="space-y-1">
          <label className="text-sm font-medium block">Quantity</label>
          <Input
            type="number"
            value={quantity ?? ""}
            onChange={(e) => {
              const v =
                e.target.value === "" ? null : parseFloat(e.target.value);
              setQuantity(v == null || isNaN(v) ? null : v);
            }}
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
            !selectedSymbolId ||
            cost == null ||
            cost < 0 ||
            quantity == null ||
            quantity < 1
          }
        >
          {isSubmitting ? "Creating..." : "Create Position"}
        </Button>
      </div>
    </ResponsiveDialog>
  );
}
