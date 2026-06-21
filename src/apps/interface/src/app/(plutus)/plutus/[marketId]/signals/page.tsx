"use client";

import ClipboardCopy from "@/components/shared/clipboard-copy";
import { PrettyNumber } from "@/components/shared/pretty-number/pretty-number";
import { ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import {
  extractFilter,
  extractSort,
} from "@/components/shared/responsive-data-table/types";
import { Typography } from "@/components/shared/typography";
import { PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { useApi } from "@/hooks/use-api";
import { GetSignalRankingsRow, plutusApi } from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";

function ScoreCell({ value }: { value: number | null | undefined }) {
  if (value == null) return <span className="text-muted-foreground">-</span>;
  const color =
    value > 0
      ? "text-green-600 dark:text-green-400"
      : value < 0
        ? "text-red-600 dark:text-red-400"
        : "text-muted-foreground";
  return <span className={color}>{value.toFixed(3)}</span>;
}

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

  const columns: ExtendedColumnDef<GetSignalRankingsRow>[] = useMemo(
    () => [
      {
        id: "symbolName",
        header: "Name",
        accessorFn: (row) => row.symbolName,
        cell: ({ cell, row }) => (
          <Link
            href={`/plutus/${marketId}/${row.original.symbolId}`}
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
        id: "symbolSubcode",
        header: "Subcode",
        accessorFn: (row) => row.symbolSubcode,
        filterConfig: {
          type: "string",
          operators: ["eq", "neq", "contains", "startsWith", "endsWith"],
        },
      },
      {
        id: "dailyAveragePrice",
        header: "Avg Price",
        accessorFn: (row) => row.dailyAveragePrice,
        cell: ({ getValue }) => {
          const v = getValue<number | null | undefined>();
          return v ? (
            <ClipboardCopy value={v}>
              <PrettyNumber number={v} />
            </ClipboardCopy>
          ) : (
            <span className="text-muted-foreground">-</span>
          );
        },
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "dailyVolume",
        header: "Volume",
        accessorFn: (row) => row.dailyVolume,
        cell: ({ getValue }) => {
          const v = getValue<number | null | undefined>();
          return v ? (
            <ClipboardCopy value={v}>
              <PrettyNumber number={v} />
            </ClipboardCopy>
          ) : (
            <span className="text-muted-foreground">-</span>
          );
        },
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "overallScore",
        header: "Overall",
        accessorFn: (row) => row.overallScore,
        cell: ({ getValue }) => <ScoreCell value={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "buyScore",
        header: "Buy",
        accessorFn: (row) => row.buyScore,
        cell: ({ getValue }) => (
          <ScoreCell value={getValue<number | null | undefined>()} />
        ),
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "sellScore",
        header: "Sell",
        accessorFn: (row) => row.sellScore,
        cell: ({ getValue }) => (
          <ScoreCell value={getValue<number | null | undefined>()} />
        ),
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "flipScore",
        header: "Flip",
        accessorFn: (row) => row.flipScore,
        cell: ({ getValue }) => (
          <ScoreCell value={getValue<number | null | undefined>()} />
        ),
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "merchScore",
        header: "Merch",
        accessorFn: (row) => row.merchScore,
        cell: ({ getValue }) => (
          <ScoreCell value={getValue<number | null | undefined>()} />
        ),
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "signalCount",
        header: "Signals",
        accessorFn: (row) => row.signalCount,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "bullishCount",
        header: "Bullish",
        accessorFn: (row) => row.bullishCount,
        cell: ({ getValue }) => (
          <span className="text-green-600 dark:text-green-400">
            {getValue<number>()}
          </span>
        ),
      },
      {
        id: "bearishCount",
        header: "Bearish",
        accessorFn: (row) => row.bearishCount,
        cell: ({ getValue }) => (
          <span className="text-red-600 dark:text-red-400">
            {getValue<number>()}
          </span>
        ),
      },
    ],
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
        state={tableState}
        onStateChange={setTableState}
        pageInfo={pageInfo}
        className="my-2"
      />
    </div>
  );
}
