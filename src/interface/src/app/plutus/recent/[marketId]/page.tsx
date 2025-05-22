"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { GET_RECENT_MARKET_TRADES } from "@/app/plutus/queries";
import useInterval from "@/hooks/use_interval";
import { useQuery } from "@urql/next";
import { formatDistance } from "date-fns";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";

type RecentTradeRowData = {
  price: number;
  volume: number;
  createdAt: Date;
  metadata: {
    symbolId: string;
    symbolName: string;
    symbolSubcode?: string | null | undefined;
  };
};

export default function RecentMarketTrades() {
  const { marketId } = useParams<{ marketId: string }>();
  const [tableState, setTableState] = usePlutusStore((state: PlutusState) => [
    state.recentTradesTableState,
    state.setRecentTradesTableState,
  ]);

  useInterval(() => reexecute(), 15000);

  const [{ data }, reexecute] = useQuery({
    query: GET_RECENT_MARKET_TRADES,
    variables: {
      marketId: marketId,
      first:
        tableState.pagination?.first ?? tableState.pagination?.pageSize ?? 10,
    },
  });

  const columns: ExtendedColumnDef<RecentTradeRowData>[] = useMemo(
    () => [
      {
        id: "metadata.symbolName",
        header: "Name",
        accessorFn: (row) => row.metadata.symbolName,
        cell: ({ cell, row }) => (
          <Link
            href={`/plutus/explorer/${marketId}/${row.original.metadata.symbolId}`}
            className="hover:underline"
          >
            {cell.getValue<string>()}
          </Link>
        ),
      },
      {
        id: "metadata.symbolSubcode",
        header: "Subcode",
        accessorFn: (row) => row.metadata.symbolSubcode,
      },
      {
        id: "price",
        header: "Price",
        accessorFn: (row) => row.price,
        cell: ({ getValue }) => (
          <ClipboardCopy value={getValue<number>()}>
            <PrettyNumber number={getValue<number>()} />
          </ClipboardCopy>
        ),
      },
      {
        id: "volume",
        header: "Volume",
        accessorFn: (row) => row.volume,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
      },
      {
        id: "createdAt",
        header: "Date",
        accessorFn: (row) => row.createdAt,
        cell: ({ getValue }) => (
          <>
            {formatDistance(getValue<Date>(), new Date(), { addSuffix: true })}
          </>
        ),
      },
    ],
    [marketId]
  );

  return (
    <div>
      <Typography variant="lead">Recent Trades</Typography>
      <ResponsiveDataTable
        columns={columns}
        data={data?.allTrades?.nodes}
        state={tableState}
        onStateChange={setTableState}
        disableFiltering
        disablePagination
        disableSorting
        className="my-2"
      />
    </div>
  );
}
