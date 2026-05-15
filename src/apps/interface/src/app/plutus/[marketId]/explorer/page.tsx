"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
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
import { useApi } from "@/hooks/use-api";
import { GetMarketTradesRow, plutusApi } from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";

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
  const filter = useMemo(
    () => extractFilter(tableState.filter),
    [tableState.filter],
  );

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getMarketTrades(marketId, timeFrameKey, {
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

  const columns: ExtendedColumnDef<GetMarketTradesRow>[] = useMemo(
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
        id: "minPrice",
        header: "Min Price",
        accessorFn: (row) => row.minPrice,
        cell: ({ getValue }) => (
          <ClipboardCopy value={getValue<number>()}>
            <PrettyNumber number={getValue<number>()} />
          </ClipboardCopy>
        ),
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "maxPrice",
        header: "Max Price",
        accessorFn: (row) => row.maxPrice,
        cell: ({ getValue }) => (
          <ClipboardCopy value={getValue<number>()}>
            <PrettyNumber number={getValue<number>()} />
          </ClipboardCopy>
        ),
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "averagePrice",
        header: "Average Price",
        accessorFn: (row) => row.averagePrice,
        cell: ({ getValue }) => (
          <ClipboardCopy value={getValue<number>()}>
            <PrettyNumber number={getValue<number>()} />
          </ClipboardCopy>
        ),
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "totalVolume",
        header: "Volume",
        accessorFn: (row) => row.totalVolume,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "limit",
        header: "Limit",
        accessorFn: (row) => row.limit,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "margin",
        header: "Margin",
        accessorFn: (row) => row.margin,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "totalGain",
        header: "Gain",
        accessorFn: (row) => row.totalGain,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "roi",
        header: "ROI",
        accessorFn: (row) => row.roi,
        cell: ({ getValue }) => <>{Math.round(getValue<number>() * 100)}%</>,
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
