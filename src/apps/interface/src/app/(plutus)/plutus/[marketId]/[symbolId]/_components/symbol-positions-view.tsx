"use client";

import { Plus } from "lucide-react";
import { useMemo, useState } from "react";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import { Typography } from "@/components/shared/typography";
import { ClosePositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/close-position-dialog";
import { CreatePositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/create-position-dialog";
import { EditPositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/edit-position-dialog";
import { LinkPositionDialog } from "@/app/(plutus)/plutus/[marketId]/_components/positions/link-position-dialog";
import { makePositionColumns } from "@/app/(plutus)/plutus/[marketId]/_components/positions/position-columns";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { type PagedResponse } from "@/lib/api-client";
import { type Position, type Symbol, plutusApi } from "@/lib/api/plutus";

export function SymbolPositionsView({
  marketId,
  symbol,
}: {
  marketId: string;
  symbol: Symbol | undefined;
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

  const [openState, reexecuteOpen] = useApi<PagedResponse<Position>>(
    () =>
      symbol?.id
        ? plutusApi.getAllPositions(marketId, {
            filter: [`and(symbolId:eq:${symbol.id}|status:eq:Pending)`],
            skip: 0,
            take: 50,
            sortField: "createdAt",
            sortDirection: "desc",
          })
        : Promise.resolve({ items: [], totalCount: 0, skip: 0, take: 0 }),
    [marketId, symbol?.id],
  );

  const [closedState, reexecuteClosed] = useApi<PagedResponse<Position>>(
    () =>
      symbol?.id
        ? plutusApi.getAllPositions(marketId, {
            filter: [`and(symbolId:eq:${symbol.id}|status:neq:Pending)`],
            skip: 0,
            take: 50,
            sortField: "createdAt",
            sortDirection: "desc",
          })
        : Promise.resolve({ items: [], totalCount: 0, skip: 0, take: 0 }),
    [marketId, symbol?.id],
  );

  const openPositions = openState.data?.items ?? [];
  const closedPositions = closedState.data?.items ?? [];

  const openPageInfo = openState.data
    ? {
        totalCount: openState.data.totalCount,
        skip: openState.data.skip,
        take: openState.data.take,
        hasNextPage:
          openState.data.skip + openState.data.take < openState.data.totalCount,
        hasPreviousPage: openState.data.skip > 0,
      }
    : undefined;

  const closedPageInfo = closedState.data
    ? {
        totalCount: closedState.data.totalCount,
        skip: closedState.data.skip,
        take: closedState.data.take,
        hasNextPage:
          closedState.data.skip + closedState.data.take <
          closedState.data.totalCount,
        hasPreviousPage: closedState.data.skip > 0,
      }
    : undefined;

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
          onLink: (p) => setLinkSellPosition(p),
          onCreateSell: (p) => setCreateSellForBuy(p),
        },
      }),
    [marketId],
  );

  const refreshAll = () => {
    reexecuteOpen();
    reexecuteClosed();
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

      {openPositions.length === 0 && closedPositions.length === 0 ? (
        <p className="mt-4 text-sm text-muted-foreground">
          No positions for this symbol.
        </p>
      ) : (
        <>
          {openPositions.length > 0 && (
            <div className="mt-4">
              <Typography variant="h4" className="text-muted-foreground mb-2">
                Open
              </Typography>
              <ResponsiveDataTable
                columns={openColumns}
                data={openPositions}
                pageInfo={openPageInfo}
                disablePagination
                disableSorting
                disableFiltering
                className="overflow-hidden"
              />
            </div>
          )}

          {closedPositions.length > 0 && (
            <div className="mt-4">
              <Typography variant="h4" className="text-muted-foreground mb-2">
                Closed
              </Typography>
              <ResponsiveDataTable
                columns={closedColumns}
                data={closedPositions}
                pageInfo={closedPageInfo}
                disablePagination
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
        defaultSymbolId={symbol?.id}
        defaultSymbolName={symbol?.name}
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
          positionId={linkSellPosition.id}
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
