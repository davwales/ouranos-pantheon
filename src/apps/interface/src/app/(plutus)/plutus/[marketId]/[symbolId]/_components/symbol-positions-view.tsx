"use client";

import { Plus } from "lucide-react";
import { useMemo, useState } from "react";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import { Typography } from "@/components/shared/typography";
import { ClosePositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/close-position-dialog";
import { CreatePositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/create-position-dialog";
import { EditPositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/edit-position-dialog";
import { defaultsForSellOf } from "@/app/(plutus)/plutus/[marketId]/_components/positions/position-defaults";
import { LinkPositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/link-position-dialog";
import { makePositionColumns } from "@/app/(plutus)/plutus/[marketId]/_components/positions/position-columns";
import { usePositionsQuery } from "@/app/(plutus)/plutus/[marketId]/portfolio/_components/use-positions-query";
import { type DataTableState } from "@/components/shared/responsive-data-table/types";
import { Button } from "@/components/ui/button";
import { type Position } from "@/lib/api/plutus";

const INITIAL_TABLE_STATE: DataTableState = {
  pagination: { pageSize: 10, skip: 0, take: 10 },
  sort: { createdAt: "DESC" },
};

export function SymbolPositionsView({
  marketId,
  symbolId,
  symbolName,
}: {
  marketId: string;
  symbolId: string;
  symbolName: string | undefined;
}) {
  const [createOpen, setCreateOpen] = useState(false);
  const [editPosition, setEditPosition] = useState<Position | null>(null);
  const [closePosition, setClosePosition] = useState<Position | null>(null);
  const [linkSellPosition, setLinkSellPosition] = useState<Position | null>(
    null,
  );
  const [createSellForBuy, setCreateSellForBuy] = useState<Position | null>(
    null,
  );

  const [openTableState, setOpenTableState] =
    useState<DataTableState>(INITIAL_TABLE_STATE);
  const [closedTableState, setClosedTableState] =
    useState<DataTableState>(INITIAL_TABLE_STATE);

  const open = usePositionsQuery(
    marketId,
    openTableState,
    `and(symbolId:eq:${symbolId}|status:eq:Pending)`,
  );
  const closed = usePositionsQuery(
    marketId,
    closedTableState,
    `and(symbolId:eq:${symbolId}|status:neq:Pending)`,
  );

  const openLoading = open.fetching && !open.data;
  const closedLoading = closed.fetching && !closed.data;
  const hasNoPositions =
    open.data?.totalCount === 0 && closed.data?.totalCount === 0;

  const openColumns = useMemo(
    () =>
      makePositionColumns(marketId, {
        showSymbol: false,
        actions: {
          variant: "open",
          onEdit: (p) => setEditPosition(p),
          onClose: (p) => setClosePosition(p),
        },
      }),
    [marketId],
  );

  const closedColumns = useMemo(
    () =>
      makePositionColumns(marketId, {
        showSymbol: false,
        actions: {
          variant: "closed",
          onEdit: (p) => setEditPosition(p),
          onLink: (p) => setLinkSellPosition(p),
          onCreateSell: (p) => setCreateSellForBuy(p),
        },
      }),
    [marketId],
  );

  const refreshAll = () => {
    open.reexecute();
    closed.reexecute();
  };

  return (
    <div className="mt-8">
      <div className="flex items-center justify-between">
        <Typography variant="h3">Positions</Typography>
        <Button size="sm" onClick={() => setCreateOpen(true)}>
          <Plus className="h-4 w-4 mr-1" />
          Create Position
        </Button>
      </div>

      {hasNoPositions ? (
        <p className="mt-4 text-sm text-muted-foreground">
          No positions for this symbol.
        </p>
      ) : (
        <>
          {(openLoading || !!open.data?.totalCount) && (
            <div className="mt-4">
              <Typography variant="h4" className="text-muted-foreground mb-2">
                Open
              </Typography>
              <ResponsiveDataTable
                columns={openColumns}
                data={open.data?.items}
                loading={openLoading}
                state={openTableState}
                onStateChange={setOpenTableState}
                pageInfo={open.pageInfo}
                disableSorting
                disableFiltering
                className="overflow-hidden"
              />
            </div>
          )}

          {(closedLoading || !!closed.data?.totalCount) && (
            <div className="mt-4">
              <Typography variant="h4" className="text-muted-foreground mb-2">
                Closed
              </Typography>
              <ResponsiveDataTable
                columns={closedColumns}
                data={closed.data?.items}
                loading={closedLoading}
                state={closedTableState}
                onStateChange={setClosedTableState}
                pageInfo={closed.pageInfo}
                disableSorting
                disableFiltering
                className="overflow-hidden"
              />
            </div>
          )}
        </>
      )}

      <CreatePositionDialog
        marketId={marketId}
        open={createOpen}
        onOpenChange={(v) => {
          setCreateOpen(v);
          if (!v) refreshAll();
        }}
        defaultSymbolId={symbolId}
        defaultSymbolName={symbolName}
      />

      {createSellForBuy && (
        <CreatePositionDialog
          marketId={marketId}
          open={!!createSellForBuy}
          onOpenChange={(v) => {
            if (!v) {
              setCreateSellForBuy(null);
              refreshAll();
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
              refreshAll();
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
              refreshAll();
            }
          }}
          onClosed={() => {
            refreshAll();
            if (closePosition.side === "Buy") {
              setCreateSellForBuy(closePosition);
            }
          }}
        />
      )}

      {linkSellPosition && (
        <LinkPositionDialog
          sellPosition={linkSellPosition}
          marketId={marketId}
          open={!!linkSellPosition}
          onOpenChange={(v) => {
            if (!v) {
              setLinkSellPosition(null);
              refreshAll();
            }
          }}
        />
      )}
    </div>
  );
}
