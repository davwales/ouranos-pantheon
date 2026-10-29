"use client";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/components/shared/responsive-data-table/types";
import { Typography } from "@/components/shared/typography";
import { type PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { errorOf, useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";
import { makeSignalRankingColumns } from "./_components/signal-ranking-columns";


export default function SignalRankingsDetail() {
  const { marketId } = useParams<{ marketId: string }>();
  const [tableState, setTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.signalRankingsTableState,
      state.setSignalRankingsTableState,
    ]),
  );

  const { sortField, sortDirection } = extractSort(tableState.sort);

  const filter = useMemo(() => extractFilter(tableState), [tableState]);

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getSignalRankings(marketId, {
        skip: tableState.pagination?.skip ?? 0,
        take: tableState.pagination?.take ?? 10,
        sortField,
        sortDirection,
        filter,
      }),
    [marketId, tableState.pagination, sortField, sortDirection, filter],
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

  const columns = useMemo(
    () => makeSignalRankingColumns(marketId),
    [marketId],
  );

  return (
    <div>
      <div className="flex items-center gap-2 justify-between">
        <Typography variant="lead">Signal Rankings</Typography>
        <div className="flex items-center gap-4">
          {fetching ? (
            <RefreshCw className="animate-spin" />
          ) : (
            <RefreshCw onClick={reexecute} className="hover:cursor-pointer" />
          )}
        </div>
      </div>

      <ResponsiveDataTable
        columns={columns}
        data={data?.items}
        loading={fetching && !data}
        error={errorOf(state)}
        onRetry={reexecute}
        state={tableState}
        onStateChange={setTableState}
        pageInfo={pageInfo}
        className="my-2"
      />
    </div>
  );
}
