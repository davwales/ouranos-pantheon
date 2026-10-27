"use client";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/components/shared/responsive-data-table/types";
import { Typography } from "@/components/shared/typography";
import TimeFrameSelection from "@/app/(plutus)/plutus/components/time-frame-selection";
import { type PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import { Plus, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";
import { makeSymbolGroupColumns } from "./_components/symbol-group-columns";

export default function GroupsPage() {
  const { marketId } = useParams<{ marketId: string }>();
  const [timeFrameKey, tableState, setTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.timeFrameKey,
      state.symbolGroupsTableState,
      state.setSymbolGroupsTableState,
    ]),
  );

  const { sortField, sortDirection } = extractSort(tableState.sort);

  const filter = useMemo(() => extractFilter(tableState), [tableState]);

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getAllSymbolGroups(marketId, timeFrameKey, {
        skip: tableState.pagination?.skip ?? 0,
        take: tableState.pagination?.take ?? 10,
        sortField,
        sortDirection,
        filter,
      }),
    [
      marketId,
      timeFrameKey,
      tableState.pagination,
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

  const columns = useMemo(
    () => makeSymbolGroupColumns(marketId),
    [marketId],
  );

  return (
    <div>
      <div className="flex items-center gap-2 justify-between">
        <div className="flex items-end gap-2">
          <Typography variant="lead">Groups</Typography>
        </div>
        <div className="flex items-center gap-4">
          <Link href={`/plutus/${marketId}/groups/create`}>
            <Button variant="link" className="flex items-end gap-0">
              <Plus className="w-4 h-4 mr-1" />
              Create Group
            </Button>
          </Link>
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
