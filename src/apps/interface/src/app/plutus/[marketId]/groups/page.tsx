"use client";

import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/app/components/responsive-data-table/types";
import { Typography } from "@/app/components/typography";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { SymbolGroup, plutusApi } from "@/lib/api/plutus";
import { Plus, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";

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
  const filter = useMemo(
    () => extractFilter(tableState.filter),
    [tableState.filter],
  );

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

  const columns: ExtendedColumnDef<SymbolGroup>[] = useMemo(
    () => [
      {
        id: "name",
        header: "Name",
        accessorFn: (row) => row.name,
        cell: ({ cell, row }) => (
          <Link
            href={`/plutus/${marketId}/groups/${row.original.id}`}
            className="hover:underline"
          >
            {cell.getValue<string>()}
          </Link>
        ),
        filterConfig: {
          type: "string",
          operators: ["eq", "neq", "contains", "startsWith", "endsWith"],
        },
      },
      {
        id: "symbolCount",
        header: "Symbols",
        accessorFn: (row) => row.symbolCount,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "totalVolume",
        header: "Total Volume",
        accessorFn: (row) => row.totalVolume,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? <PrettyNumber number={val} /> : "-";
        },
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "totalGain",
        header: "Total Gain",
        accessorFn: (row) => row.totalGain,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? <PrettyNumber number={val} /> : "-";
        },
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "averageRoi",
        header: "Avg ROI",
        accessorFn: (row) => row.averageRoi,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? `${(val * 100).toFixed(2)}%` : "-";
        },
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "averageOverallScore",
        header: "Avg Signal",
        accessorFn: (row) => row.averageOverallScore,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? val.toFixed(3) : "-";
        },
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "bullishCount",
        header: "Bullish",
        accessorFn: (row) => row.bullishCount,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "bearishCount",
        header: "Bearish",
        accessorFn: (row) => row.bearishCount,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
    ],
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
        state={tableState}
        onStateChange={setTableState}
        pageInfo={pageInfo}
        className="my-2"
      />
    </div>
  );
}
