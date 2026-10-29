"use client";

import { type Position } from "@/lib/api/plutus";
import { ClosePositionDialog } from "../../_components/positions/close-position-dialog";
import { CreatePositionDialog } from "../../_components/positions/create-position-dialog";
import { EditPositionDialog } from "../../_components/positions/edit-position-dialog";
import { LinkPositionDialog } from "../../_components/positions/link-position-dialog";
import { defaultsForSellOf } from "../../_components/positions/position-defaults";

type PositionActionDialogsProps = {
  marketId: string;
  createSellForBuy: Position | null;
  editPosition: Position | null;
  closePosition: Position | null;
  linkSellPosition: Position | null;
  setCreateSellForBuy: (position: Position | null) => void;
  setEditPosition: (position: Position | null) => void;
  setClosePosition: (position: Position | null) => void;
  setLinkSellPosition: (position: Position | null) => void;
  onChanged: () => void;
};

/** Dialogs opened from the row actions of the open and closed position tables. */
export function PositionActionDialogs({
  marketId,
  createSellForBuy,
  editPosition,
  closePosition,
  linkSellPosition,
  setCreateSellForBuy,
  setEditPosition,
  setClosePosition,
  setLinkSellPosition,
  onChanged,
}: PositionActionDialogsProps) {
  return (
    <>
      {createSellForBuy && (
        <CreatePositionDialog
          marketId={marketId}
          open={!!createSellForBuy}
          onOpenChange={(v) => {
            if (!v) {
              setCreateSellForBuy(null);
              onChanged();
            }
          }}
          linkedBuyPositionId={createSellForBuy.id}
          defaultSymbolId={createSellForBuy.symbolId}
          defaultSymbolName={createSellForBuy.symbolName}
          defaults={defaultsForSellOf(createSellForBuy)}
        />
      )}

      {editPosition && (
        <EditPositionDialog
          position={editPosition}
          open={!!editPosition}
          onOpenChange={(v) => {
            if (!v) {
              setEditPosition(null);
              onChanged();
            }
          }}
        />
      )}

      {closePosition && (
        <ClosePositionDialog
          position={closePosition}
          open={!!closePosition}
          onOpenChange={(v) => {
            if (!v) {
              setClosePosition(null);
              onChanged();
            }
          }}
          onClosed={() => {
            onChanged();
            if (closePosition.side === "Buy") {
              setCreateSellForBuy(closePosition);
            }
          }}
        />
      )}

      {linkSellPosition && (
        <LinkPositionDialog
          positionId={linkSellPosition.id}
          marketId={marketId}
          open={!!linkSellPosition}
          onOpenChange={(v) => {
            if (!v) {
              setLinkSellPosition(null);
              onChanged();
            }
          }}
        />
      )}
    </>
  );
}
