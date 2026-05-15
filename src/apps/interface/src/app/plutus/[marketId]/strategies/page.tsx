"use client";

import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/app/components/responsive-data-table/types";
import { Typography } from "@/app/components/typography";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { Strategy, plutusApi } from "@/lib/api/plutus";
import { Plus, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";

const strategyTypeLabels: Record<string, string> = {
  SignalWeighted: "Signal Weighted",
  ForecastMomentum: "Forecast Momentum",
  MeanReversion: "Mean Reversion",
  RecipeArbitrage: "Recipe Arbitrage",
  Composite: "Composite",
};

export default function StrategiesPage() {
  const { marketId } = useParams<{ marketId: string }>();
  const [tableState, setTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.strategiesTableState,
      state.setStrategiesTableState,
    ]),
  );

  const { sortField, sortDirection } = extractSort(tableState.sort);
  const filter = useMemo(
    () => extractFilter(tableState.filter),
    [tableState.filter],
  );

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getAllStrategies(marketId, {
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

  const columns: ExtendedColumnDef<Strategy>[] = useMemo(
    () => [
      {
        id: "name",
        header: "Name",
        accessorFn: (row) => row.name,
        cell: ({ cell, row }) => (
          <Link
            href={`/plutus/${marketId}/strategies/${row.original.id}`}
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
        id: "type",
        header: "Type",
        accessorFn: (row) => strategyTypeLabels[row.type] ?? row.type,
      },
      {
        id: "backtestCount",
        header: "Backtests",
        accessorFn: (row) => row.backtestCount,
      },
      {
        id: "lastBacktestReturn",
        header: "Last Return",
        accessorFn: (row) => row.lastBacktestReturn,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          if (val == null) return "-";
          const color = val >= 0 ? "text-green-600" : "text-red-600";
          return <span className={color}>{(val * 100).toFixed(2)}%</span>;
        },
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "lastBacktestWinRate",
        header: "Win Rate",
        accessorFn: (row) => row.lastBacktestWinRate,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? `${(val * 100).toFixed(1)}%` : "-";
        },
      },
      {
        id: "isActive",
        header: "Active",
        accessorFn: (row) => row.isActive,
        cell: ({ getValue }) => (getValue<boolean>() ? "Yes" : "No"),
        filterConfig: {
          type: "boolean",
          operators: ["eq"],
        },
      },
      {
        id: "createdAt",
        header: "Created",
        accessorFn: (row) => row.createdAt,
        cell: ({ getValue }) =>
          new Date(getValue<string>()).toLocaleDateString(),
      },
    ],
    [marketId],
  );

  return (
    <div>
      <div className="flex items-center gap-2 justify-between">
        <Typography variant="lead">Strategies</Typography>
        <div className="flex items-center gap-4">
          <Link href={`/plutus/${marketId}/strategies/create`}>
            <Button variant="link" className="flex items-end gap-0">
              <Plus className="w-4 h-4 mr-1" />
              Create Strategy
            </Button>
          </Link>
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
        state={tableState}
        onStateChange={setTableState}
        pageInfo={pageInfo}
        className="my-2"
      />
    </div>
  );
}
