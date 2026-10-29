"use client";

import { SymbolSelect } from "@/app/(plutus)/plutus/components/symbol-select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { NumericInput } from "@/components/shared/numeric-input";
import { type PositionSide, plutusApi } from "@/lib/api/plutus";
import { useState } from "react";
import { type PositionDefaults } from "./position-defaults";

export function CreatePositionForm({
  marketId,
  onDone,
  linkedBuyPositionId,
  defaultSymbolId,
  defaultSymbolName,
  defaults,
}: {
  marketId: string;
  onDone: () => void;
  linkedBuyPositionId?: string | null;
  defaultSymbolId?: string;
  defaultSymbolName?: string;
  defaults?: PositionDefaults;
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
  const [cost, setCost] = useState<number | null>(defaults?.cost ?? null);
  const [quantity, setQuantity] = useState<number | null>(
    defaults?.quantity ?? null,
  );
  const [targetPrice, setTargetPrice] = useState<number | null>(
    defaults?.targetPrice ?? null,
  );
  const [notes, setNotes] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isSymbolReadOnly = !!defaultSymbolId;

  const handleSubmit = async () => {
    if (cost == null || quantity == null || !selectedSymbolId) return;

    setIsSubmitting(true);
    setError(null);
    try {
      await plutusApi.createPosition({
        side,
        marketId,
        symbolId: selectedSymbolId,
        cost,
        quantity,
        strategyId: defaults?.strategyId ?? null,
        notes: notes.trim() || null,
        targetPrice: side === "Buy" ? targetPrice : null,
        linkedBuyPositionId: linkedBuyPositionId ?? null,
      });

      onDone();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create position",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
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
      {side === "Buy" && (
        <div className="space-y-1">
          <NumericInput
            label="Target sell price"
            value={targetPrice}
            onChange={setTargetPrice}
            min={0}
            step={0.01}
          />
        </div>
      )}
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
  );
}
