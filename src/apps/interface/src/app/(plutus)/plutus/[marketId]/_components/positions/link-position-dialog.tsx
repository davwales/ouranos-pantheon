"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { type Position, plutusApi } from "@/lib/api/plutus";
import { useState } from "react";

export function LinkPositionDialog({
  sellPosition,
  marketId,
  open,
  onOpenChange,
}: {
  sellPosition: Position;
  marketId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const [selectedBuyPositionId, setSelectedBuyPositionId] = useState<
    string | null
  >(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [state] = useApi(
    () =>
      plutusApi.getAllPositions(marketId, {
        skip: 0,
        take: 100,
        filter: linkableBuyFilter(sellPosition),
      }),
    [marketId, sellPosition.symbolId, sellPosition.quantity, open],
  );

  const buyPositions = state.data?.items ?? [];

  const handleLink = async () => {
    if (!selectedBuyPositionId) return;

    setIsSubmitting(true);
    setError(null);
    try {
      await plutusApi.linkPosition(sellPosition.id, selectedBuyPositionId);
      onOpenChange(false);
      setSelectedBuyPositionId(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to link position");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ResponsiveDialog
      title="Link Position"
      description="Select a position to link to."
      open={open}
      onOpenChange={(v) => {
        onOpenChange(v);
        if (!v) setSelectedBuyPositionId(null);
      }}
      trigger={null}
    >
      <div className="space-y-4">
        {error && (
          <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
            {error}
          </div>
        )}
        <div className="space-y-2">
          {buyPositions.length === 0 && (
            <p className="text-sm text-muted-foreground">
              No bought {sellPosition.symbolName} positions with{" "}
              {sellPosition.quantity} unsold.
            </p>
          )}
          {buyPositions.map((position) => (
            <button
              key={position.id}
              type="button"
              onClick={() => setSelectedBuyPositionId(position.id)}
              className={`w-full text-left rounded-lg border p-3 transition-colors ${
                selectedBuyPositionId === position.id
                  ? "border-primary bg-primary/5"
                  : "hover:bg-muted/50"
              }`}
            >
              <div className="font-medium text-sm">{position.symbolName}</div>
              <div className="text-xs text-muted-foreground">
                {position.quantity} @ {position.cost} ·{" "}
                {position.unsoldQuantity ?? position.quantity} unsold
              </div>
            </button>
          ))}
        </div>
        <Button
          className="w-full"
          onClick={handleLink}
          disabled={isSubmitting || !selectedBuyPositionId}
        >
          {isSubmitting ? "Linking..." : "Link Position"}
        </Button>
      </div>
    </ResponsiveDialog>
  );
}

/** Server-side filter for the Bought buys this sell can close out: same symbol, enough unsold. */
export function linkableBuyFilter(sellPosition: Position): string[] {
  return [
    "side:eq:Buy",
    "status:eq:Bought",
    `symbolId:eq:${sellPosition.symbolId}`,
    `unsoldQuantity:gte:${sellPosition.quantity}`,
  ];
}
