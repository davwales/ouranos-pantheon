"use client";

import { explorerColumns } from "@/app/(plutus)/plutus/[marketId]/explorer/_components/explorer-columns";
import { MarketAnalystButton } from "@/app/(plutus)/plutus/[marketId]/_components/market-analyst-button";
import TimeFrameSelection from "@/app/(plutus)/plutus/components/time-frame-selection";
import { useNavBarActions } from "@/components/shared/nav-bar-actions-context";
import { NotFoundCard } from "@/components/shared/not-found-card";
import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/components/shared/responsive-data-table/types";
import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import { PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { RefreshCw } from "lucide-react";
import { useParams } from "next/navigation";
import { useEffect, useMemo } from "react";
import { useShallow } from "zustand/react/shallow";

const SUGGESTIONS = [
  "Which of these is the best opportunity?",
  "Which of these are too illiquid to trust?",
  "Compare the top three after tax.",
];

export default function MarketDetail() {
  const { marketId } = useParams<{ marketId: string }>();
  const [timeFrameKey, tableState, setTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.timeFrameKey,
      state.explorerTableState,
      state.setExplorerTableState,
    ]),
  );

  const { sortField, sortDirection } = extractSort(tableState.sort);

  const filter = useMemo(() => extractFilter(tableState), [tableState]);
  const skip = tableState.pagination?.skip ?? 0;
  const take = tableState.pagination?.take ?? 10;

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getMarketTrades(marketId, timeFrameKey, {
        skip,
        take,
        sortField,
        sortDirection,
        filter,
      }),
    [
      marketId,
      timeFrameKey,
      skip,
      take,
      sortField,
      sortDirection,
      filter,
    ],
  );

  const data = state.data;
  const fetching = state.status === "loading";

  const pageInfo = data
    ? {
        totalCount: data.totalCount,
        skip: data.skip,
        take: data.take,
        hasNextPage: data.skip + data.take < data.totalCount,
        hasPreviousPage: data.skip > 0,
      }
    : undefined;

  const columns = useMemo(() => explorerColumns(marketId), [marketId]);

  const { setActions, clearActions } = useNavBarActions();

  useEffect(() => {
    setActions(
      <MarketAnalystButton
        key={`${marketId}:${timeFrameKey}`}
        context={{
          marketId,
          timeFrame: timeFrameKey,
          filter,
          sortField,
          sortDirection,
          skip,
          take,
        }}
        suggestions={SUGGESTIONS}
      />,
    );
    return () => clearActions();
  }, [
    marketId,
    timeFrameKey,
    filter,
    sortField,
    sortDirection,
    skip,
    take,
    setActions,
    clearActions,
  ]);

  if (state.status === "error" && !data) {
    return <NotFoundCard title="Explorer data not found" backHref={`/plutus/${marketId}`} backLabel="Back to Market" />;
  }

  return (
    <div>
      <div className="flex items-center gap-2 justify-between">
        <div className="flex items-end gap-2">
          <Typography variant="lead">Explorer</Typography>
        </div>
        <div className="flex items-center gap-4">
          {fetching ? (
            <RefreshCw className="animate-spin" />
          ) : (
            <RefreshCw onClick={reexecute} className="hover:cursor-pointer" />
          )}
          <TimeFrameSelection />
        </div>
      </div>

      <ResponsiveDataTable
        columns={columns}
        data={data?.items}
        loading={fetching && !data}
        state={tableState}
        onStateChange={setTableState}
        pageInfo={pageInfo}
        className="my-2"
      />
    </div>
  );
}
