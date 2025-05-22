"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { GET_MARKET_TRADES } from "@/app/plutus/queries";
import { GetMarketTradesResponse } from "@/gql/graphql";
import { useQuery } from "@urql/next";
import { RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";

export default function MarketDetail() {
  const { marketId } = useParams<{ marketId: string }>();
  const [timeFrameSeconds, tableState, setTableState] = usePlutusStore(
    (state: PlutusState) => [
      state.timeFrameSeconds,
      state.explorerTableState,
      state.setExplorerTableState,
    ]
  );

  const [{ data, fetching }, reexecute] = useQuery({
    query: GET_MARKET_TRADES,
    variables: {
      input: {
        marketId: marketId,
        seconds: timeFrameSeconds > 0 ? timeFrameSeconds : undefined,
      },
      order: tableState.sort,
      where: tableState.filter,
      first: tableState.pagination?.first,
      after: tableState.pagination?.after,
      last: tableState.pagination?.last,
      before: tableState.pagination?.before,
    },
  });

  const columns: ExtendedColumnDef<GetMarketTradesResponse>[] = useMemo(
    () => [
      {
        id: "symbolName",
        header: "Name",
        accessorFn: (row) => row.symbolName,
        cell: ({ cell, row }) => (
          <Link
            href={`/plutus/explorer/${marketId}/${row.original.symbolId}`}
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
    [marketId]
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
        data={data?.marketTrades?.nodes}
        state={tableState}
        onStateChange={setTableState}
        pageInfo={data?.marketTrades?.pageInfo}
        className="my-2"
      />
    </div>
  );
}
