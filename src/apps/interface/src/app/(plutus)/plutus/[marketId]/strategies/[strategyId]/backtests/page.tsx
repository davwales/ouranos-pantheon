"use client";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/components/shared/responsive-data-table/types";
import { Typography } from "@/components/shared/typography";
import { type PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { Button } from "@/components/ui/button";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useApi } from "@/hooks/use-api";
import { type StrategyDetail, plutusApi } from "@/lib/api/plutus";
import { Play, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";
import { makeBacktestColumns } from "./_components/backtest-columns";

function BacktestsActions({
  marketId,
  strategyId,
  fetching,
  onRefresh,
}: {
  marketId: string;
  strategyId: string;
  fetching: boolean;
  onRefresh: () => void;
}) {
  return (
    <div className="flex items-center gap-4 justify-end shrink-0">
      <Link href={`/plutus/${marketId}/strategies/${strategyId}`}>
        <Button variant="link" className="flex items-end gap-0">
          <Play className="w-4 h-4 mr-1" />
          Run Backtest
        </Button>
      </Link>
      {fetching ? (
        <RefreshCw className="animate-spin" />
      ) : (
        <RefreshCw onClick={onRefresh} className="hover:cursor-pointer" />
      )}
    </div>
  );
}

function EmptyBacktestsState({
  marketId,
  strategyId,
}: {
  marketId: string;
  strategyId: string;
}) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-muted-foreground">
      <Typography variant="h3" className="mb-2">
        No backtests yet
      </Typography>
      <p className="text-sm mb-4">
        Run a backtest or optimization to see results here.
      </p>
      <Link href={`/plutus/${marketId}/strategies/${strategyId}`}>
        <Button variant="outline">
          <Play className="w-4 h-4 mr-1" />
          Run Backtest
        </Button>
      </Link>
    </div>
  );
}

export default function BacktestsPage() {
  const { marketId, strategyId } = useParams<{
    marketId: string;
    strategyId: string;
  }>();

  const [tableState, setTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.backtestsTableState,
      state.setBacktestsTableState,
    ]),
  );

  const { sortField, sortDirection } = extractSort(tableState.sort);
  const filter = useMemo(() => extractFilter(tableState), [tableState]);

  const [strategyState] = useApi<StrategyDetail>(
    () => plutusApi.getStrategy(strategyId),
    [strategyId],
  );
  useBreadcrumbLabel(strategyId, strategyState.data?.name, strategyState.status === "error");

  const [backtestsState, reexecute] = useApi(
    () =>
      plutusApi.getAllBacktests(strategyId, {
        skip: tableState.pagination?.skip ?? 0,
        take: tableState.pagination?.take ?? 10,
        sortField,
        sortDirection,
        filter,
      }),
    [strategyId, tableState.pagination, sortField, sortDirection, filter],
  );

  const data = backtestsState.data;
  const fetching = backtestsState.status === "loading";

  const pageInfo = data
    ? {
        totalCount: data.totalCount,
        skip: data.skip,
        take: data.take,
        hasNextPage: data.skip + data.take < data.totalCount,
        hasPreviousPage: data.skip > 0,
      }
    : undefined;

  const columns = useMemo(
    () => makeBacktestColumns(marketId, strategyId),
    [marketId, strategyId],
  );

  return (
    <div>
      <BacktestsActions
        marketId={marketId}
        strategyId={strategyId}
        fetching={fetching}
        onRefresh={reexecute}
      />

      {data && data.items.length === 0 ? (
        <EmptyBacktestsState marketId={marketId} strategyId={strategyId} />
      ) : (
        <ResponsiveDataTable
          columns={columns}
          data={data?.items}
          loading={fetching && !data}
          state={tableState}
          onStateChange={setTableState}
          pageInfo={pageInfo}
          className="my-2"
        />
      )}
    </div>
  );
}
