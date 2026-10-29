"use client";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import { Typography } from "@/components/shared/typography";
import { type PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { useApi } from "@/hooks/use-api";
import {
  type Position,
  type StrategyRecommendation,
  plutusApi,
} from "@/lib/api/plutus";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";
import { useShallow } from "zustand/react/shallow";
import { CreatePositionDialog } from "../_components/positions/create-position-dialog";
import { makePositionColumns } from "../_components/positions/position-columns";
import {
  type PositionDefaults,
  defaultsFromRecommendation,
} from "../_components/positions/position-defaults";
import { PortfolioHeader } from "./_components/portfolio-header";
import { PositionActionDialogs } from "./_components/position-action-dialogs";
import { RecommendationsPanel } from "./_components/recommendations-panel";
import { usePositionsQuery } from "./_components/use-positions-query";

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

  const {
    data: openData,
    fetching: fetchingOpen,
    pageInfo: openPageInfo,
    reexecute: reexecuteOpen,
  } = usePositionsQuery(marketId, openTableState, "status:eq:Pending");

  const {
    data: closedData,
    fetching: fetchingClosed,
    pageInfo: closedPageInfo,
    reexecute: reexecuteClosed,
  } = usePositionsQuery(marketId, closedTableState, "status:neq:Pending");

  const [createOpen, setCreateOpen] = useState(false);
  const [createDefaultSymbolId, setCreateDefaultSymbolId] = useState<
    string | undefined
  >(undefined);
  const [createDefaultSymbolName, setCreateDefaultSymbolName] = useState<
    string | undefined
  >(undefined);
  const [createDefaults, setCreateDefaults] = useState<
    PositionDefaults | undefined
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
    () => plutusApi.getAllStrategies(marketId, { take: 100 }),
    [marketId],
  );

  const handleCreateFromRecommendation = (
    recommendation: StrategyRecommendation,
    strategyId: string,
  ) => {
    setCreateDefaultSymbolId(recommendation.symbolId);
    setCreateDefaultSymbolName(recommendation.symbolName);
    setCreateDefaults(defaultsFromRecommendation(recommendation, strategyId));
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
          onEdit: (p) => setEditPosition(p),
          onLink: (p) => setLinkSellPosition(p),
          onCreateSell: (p) => setCreateSellForBuy(p),
        },
      }),
    [marketId],
  );

  return (
    <div>
      <PortfolioHeader
        fetching={fetchingOpen}
        onCreatePosition={() => setCreateOpen(true)}
        onRefresh={reexecuteOpen}
      />

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
          setCreateDefaults(undefined);
          if (!v) refreshAll();
        }}
        defaultSymbolId={createDefaultSymbolId}
        defaultSymbolName={createDefaultSymbolName}
        defaults={createDefaults}
      />

      <PositionActionDialogs
        marketId={marketId}
        createSellForBuy={createSellForBuy}
        editPosition={editPosition}
        closePosition={closePosition}
        linkSellPosition={linkSellPosition}
        setCreateSellForBuy={setCreateSellForBuy}
        setEditPosition={setEditPosition}
        setClosePosition={setClosePosition}
        setLinkSellPosition={setLinkSellPosition}
        onChanged={refreshAll}
      />
    </div>
  );
}
