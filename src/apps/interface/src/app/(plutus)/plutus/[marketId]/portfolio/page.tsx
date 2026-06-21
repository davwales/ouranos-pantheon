"use client";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/components/shared/responsive-data-table/types";
import { Typography } from "@/components/shared/typography";
import { PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { Position, plutusApi } from "@/lib/api/plutus";
import { Plus, RefreshCw } from "lucide-react";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";
import { useShallow } from "zustand/react/shallow";
import { ClosePositionDialog } from "../_components/positions/close-position-dialog";
import { CreatePositionDialog } from "../_components/positions/create-position-dialog";
import { EditPositionDialog } from "../_components/positions/edit-position-dialog";
import { LinkPositionDialog } from "../_components/positions/link-position-dialog";
import { makePositionColumns } from "../_components/positions/position-columns";
import { RecommendationsPanel } from "./_components/recommendations-panel";

export default function PortfolioPage() {
  const { marketId } = useParams<{ marketId: string }>();
  const [openTableState, setOpenTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.openPositionsTableState,
      state.setOpenPositionsTableState,
    ]),
  );
  const [closedTableState, setClosedTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.closedPositionsTableState,
      state.setClosedPositionsTableState,
    ]),
  );

  const openSort = extractSort(openTableState.sort);
  const openFilter = useMemo(
    () => [...(extractFilter(openTableState) ?? []), "status:eq:Pending"],
    [openTableState],
  );

  const closedSort = extractSort(closedTableState.sort);
  const closedFilter = useMemo(
    () => [...(extractFilter(closedTableState) ?? []), "status:neq:Pending"],
    [closedTableState],
  );

  const [openState, reexecuteOpen] = useApi(
    () =>
      plutusApi.getAllPositions(marketId, {
        skip: openTableState.pagination?.skip ?? 0,
        take: openTableState.pagination?.take ?? 10,
        sortField: openSort.sortField,
        sortDirection: openSort.sortDirection,
        filter: openFilter,
      }),
    [
      marketId,
      openTableState.pagination,
      openSort.sortField,
      openSort.sortDirection,
      openFilter,
    ],
  );

  const [closedState, reexecuteClosed] = useApi(
    () =>
      plutusApi.getAllPositions(marketId, {
        skip: closedTableState.pagination?.skip ?? 0,
        take: closedTableState.pagination?.take ?? 10,
        sortField: closedSort.sortField,
        sortDirection: closedSort.sortDirection,
        filter: closedFilter,
      }),
    [
      marketId,
      closedTableState.pagination,
      closedSort.sortField,
      closedSort.sortDirection,
      closedFilter,
    ],
  );

  const openData = openState.data;
  const fetchingOpen = openState.status === "loading";
  const openPageInfo = openData
    ? {
        totalCount: openData.totalCount,
        skip: openData.skip,
        take: openData.take,
        hasNextPage: openData.skip + openData.take < openData.totalCount,
        hasPreviousPage: openData.skip > 0,
      }
    : undefined;

  const closedData = closedState.data;
  const fetchingClosed = closedState.status === "loading";
  const closedPageInfo = closedData
    ? {
        totalCount: closedData.totalCount,
        skip: closedData.skip,
        take: closedData.take,
        hasNextPage: closedData.skip + closedData.take < closedData.totalCount,
        hasPreviousPage: closedData.skip > 0,
      }
    : undefined;

  const [createOpen, setCreateOpen] = useState(false);
  const [createDefaultSymbolId, setCreateDefaultSymbolId] = useState<
    string | undefined
  >(undefined);
  const [createDefaultSymbolName, setCreateDefaultSymbolName] = useState<
    string | undefined
  >(undefined);
  const [editPosition, setEditPosition] = useState<Position | null>(null);
  const [closePosition, setClosePosition] = useState<Position | null>(null);
  const [linkSellPosition, setLinkSellPosition] = useState<Position | null>(
    null,
  );
  const [createSellForBuy, setCreateSellForBuy] = useState<Position | null>(
    null,
  );

  const [strategiesState] = useApi(
    () => plutusApi.getAllStrategies(marketId),
    [marketId],
  );

  const handleCreateFromRecommendation = (
    symbolId: string,
    symbolName: string,
  ) => {
    setCreateDefaultSymbolId(symbolId);
    setCreateDefaultSymbolName(symbolName);
    setCreateOpen(true);
  };

  const refreshAll = () => {
    reexecuteOpen();
    reexecuteClosed();
  };

  const openColumns = useMemo(
    () =>
      makePositionColumns(marketId, {
        showSymbol: true,
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
        showSymbol: true,
        actions: {
          variant: "closed",
          onLink: (p) => setLinkSellPosition(p),
          onCreateSell: (p) => setCreateSellForBuy(p),
        },
      }),
    [marketId],
  );

  return (
    <div>
      <div className="flex items-center gap-2 justify-between">
        <Typography variant="lead">Portfolio</Typography>
        <div className="flex items-center gap-4">
          <Button
            variant="link"
            className="flex items-end gap-0"
            onClick={() => setCreateOpen(true)}
          >
            <Plus className="w-4 h-4 mr-1" />
            Create Position
          </Button>
          {fetchingOpen ? (
            <RefreshCw className="animate-spin" />
          ) : (
            <RefreshCw
              onClick={reexecuteOpen}
              className="hover:cursor-pointer"
            />
          )}
        </div>
      </div>

      <Typography variant="h4" className="mt-4 mb-2">
        Open Positions
      </Typography>
      <ResponsiveDataTable
        columns={openColumns}
        data={openData?.items}
        loading={fetchingOpen && !openData}
        state={openTableState}
        onStateChange={setOpenTableState}
        pageInfo={openPageInfo}
        className="overflow-hidden"
      />

      <Typography variant="h4" className="mt-8 mb-2">
        Closed Positions
      </Typography>
      <ResponsiveDataTable
        columns={closedColumns}
        data={closedData?.items}
        loading={fetchingClosed && !closedData}
        state={closedTableState}
        onStateChange={setClosedTableState}
        pageInfo={closedPageInfo}
        className="overflow-hidden"
      />

      {strategiesState.data && (
        <RecommendationsPanel
          marketId={marketId}
          strategies={strategiesState.data.items}
          onCreatePosition={handleCreateFromRecommendation}
        />
      )}

      <CreatePositionDialog
        marketId={marketId}
        open={createOpen}
        onOpenChange={(v) => {
          setCreateOpen(v);
          setCreateDefaultSymbolId(undefined);
          setCreateDefaultSymbolName(undefined);
          if (!v) refreshAll();
        }}
        defaultSymbolId={createDefaultSymbolId}
        defaultSymbolName={createDefaultSymbolName}
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
