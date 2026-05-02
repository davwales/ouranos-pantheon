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
import {
  BacktestStatus,
  BacktestSummary,
  StrategyDetail,
  plutusApi,
} from "@/lib/api/plutus";
import { Play, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";
import { StatusChip } from "../../../backtests/components/status-chip";

function BacktestsBreadcrumb({
  strategy,
  marketId,
  strategyId,
  fetching,
  onRefresh,
}: {
  strategy: StrategyDetail | null | undefined;
  marketId: string;
  strategyId: string;
  fetching: boolean;
  onRefresh: () => void;
}) {
  return (
    <div className="flex items-center gap-2 justify-between">
      <div className="flex items-center gap-2 min-w-0">
        <Link
          href={`/plutus/${marketId}/strategies/${strategyId}`}
          className="text-muted-foreground hover:text-foreground hover:underline text-sm shrink-0"
        >
          {strategy?.name ?? "Strategy"}
        </Link>
        <span className="text-muted-foreground shrink-0">/</span>
        <Typography variant="lead">Backtests</Typography>
      </div>
      <div className="flex items-center gap-4 shrink-0">
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
  const filter = useMemo(
    () => extractFilter(tableState.filter),
    [tableState.filter],
  );

  const [strategyState] = useApi<StrategyDetail>(
    () => plutusApi.getStrategy(strategyId),
    [strategyId],
  );

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

  const strategy = strategyState.data;
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

  const columns: ExtendedColumnDef<BacktestSummary>[] = useMemo(
    () => [
      {
        id: "status",
        header: "Status",
        accessorFn: (row) => row.status,
        cell: ({ row, getValue }) => (
          <Link
            href={`/plutus/${marketId}/backtests/${row.original.id}`}
            className="hover:underline"
          >
            <StatusChip status={getValue<BacktestStatus>()} />
          </Link>
        ),
      },
      {
        id: "startDate",
        header: "Start Date",
        accessorFn: (row) => row.startDate,
        cell: ({ getValue }) =>
          new Date(getValue<string>()).toLocaleDateString(),
      },
      {
        id: "endDate",
        header: "End Date",
        accessorFn: (row) => row.endDate,
        cell: ({ getValue }) =>
          new Date(getValue<string>()).toLocaleDateString(),
      },
      {
        id: "budget",
        header: "Budget",
        accessorFn: (row) => row.budget,
        cell: ({ getValue }) => getValue<number>().toLocaleString(),
      },
      {
        id: "totalReturnPercent",
        header: "Return %",
        accessorFn: (row) => row.totalReturnPercent,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          if (val == null) return "-";
          const color = val >= 0 ? "text-green-600" : "text-red-600";
          return <span className={color}>{(val * 100).toFixed(2)}%</span>;
        },
      },
      {
        id: "winRate",
        header: "Win Rate",
        accessorFn: (row) => row.winRate,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? `${(val * 100).toFixed(1)}%` : "-";
        },
      },
      {
        id: "sharpeRatio",
        header: "Sharpe",
        accessorFn: (row) => row.sharpeRatio,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? val.toFixed(2) : "-";
        },
      },
      {
        id: "totalTrades",
        header: "Trades",
        accessorFn: (row) => row.totalTrades,
        cell: ({ getValue }) => getValue<number | null>() ?? "-",
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
      <BacktestsBreadcrumb
        strategy={strategy}
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
          state={tableState}
          onStateChange={setTableState}
          pageInfo={pageInfo}
          className="my-2"
        />
      )}
    </div>
  );
}
