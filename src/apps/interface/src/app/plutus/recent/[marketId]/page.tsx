"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use_interval";
import { Trade, plutusApi } from "@/lib/api/plutus";
import { formatDistance } from "date-fns";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";

export default function RecentMarketTrades() {
  const { marketId } = useParams<{ marketId: string }>();
  const [tableState, setTableState] = usePlutusStore((state: PlutusState) => [
    state.recentTradesTableState,
    state.setRecentTradesTableState,
  ]);

  const take =
    tableState.pagination?.take ?? tableState.pagination?.pageSize ?? 10;

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getAllTrades({
        filter: [`MarketId:eq:${marketId}`],
        sortField: "Timestamp",
        sortDirection: "desc",
        skip: 0,
        take,
      }),
    [marketId, take],
  );

  useInterval(() => reexecute(), 15000);

  const columns: ExtendedColumnDef<Trade>[] = useMemo(
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
      },
      {
        id: "symbolCode",
        header: "Code",
        accessorFn: (row) => row.symbolCode,
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
        id: "timestamp",
        header: "Timestamp",
        accessorFn: (row) => row.timestamp,
        cell: ({ getValue }) => (
          <>
            {formatDistance(new Date(getValue<string>()), new Date(), {
              addSuffix: true,
            })}
          </>
        ),
      },
    ],
    [marketId],
  );

  return (
    <div>
      <Typography variant="lead">Recent Trades</Typography>
      <ResponsiveDataTable
        columns={columns}
        data={state.data}
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
