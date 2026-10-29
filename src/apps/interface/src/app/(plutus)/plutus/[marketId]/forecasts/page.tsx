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
import { formatDistance } from "date-fns";
import { RefreshCw } from "lucide-react";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";
import { makeMarketForecastColumns } from "./_components/market-forecast-columns";

export default function RecentMarketTrades() {
  const { marketId } = useParams<{ marketId: string }>();
  const [tableState, setTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.forecastsTableState,
      state.setForecastsTableState,
    ]),
  );

  const { sortField, sortDirection } = extractSort(tableState.sort);

  const filter = useMemo(() => extractFilter(tableState), [tableState]);

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getMarketForecasts(marketId, {
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
    () => makeMarketForecastColumns(marketId),
    [marketId],
  );

  const timeToRefresh = (): string => {
    const now = new Date();
    const midnightUTC = new Date(
      Date.UTC(
        now.getUTCFullYear(),
        now.getUTCMonth(),
        now.getUTCDate() + 1,
        0,
        0,
        0,
        0,
      ),
    );

    return formatDistance(midnightUTC, now, { addSuffix: true });
  };

  return (
    <div>
      <div className="flex items-end gap-2 justify-between">
        <div className="flex items-end gap-2">
          <Typography variant="lead">Forecasts</Typography>
        </div>
        <div className="flex items-center gap-4">
          <Typography variant="small">
            Next forecasts generated {timeToRefresh()}
          </Typography>
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
