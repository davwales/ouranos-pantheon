"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef, PaginationArgs } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import { GET_RECENT_MARKET_TRADES } from "@/app/plutus/queries";
import useInterval from "@/hooks/use_interval";
import { useQuery } from "@urql/next";
import { formatDistance } from "date-fns";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";

type RecentTradeRowData = {
    price: number;
    volume: number;
    createdAt: Date;
    metadata: {
        symbolId: string;
        symbolName: string;
        symbolSubcode?: string | null | undefined;
    };
}

export default function RecentMarketTrades() {
    const { marketId } = useParams<{ marketId: string }>();
    const [paginationArgs, setPaginationArgs] = useState<PaginationArgs>({ pageSize: 10 });

    useInterval(() => reexecute(), 15000);

    const [{ data, fetching }, reexecute] = useQuery({
        query: GET_RECENT_MARKET_TRADES,
        variables: {
            marketId: marketId,
            first: paginationArgs.first ?? paginationArgs.pageSize ?? 10
        }
    });

    const columns: ExtendedColumnDef<RecentTradeRowData>[] = useMemo(() => [
        {
            id: "symbolName",
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
            id: "symbolSubcode",
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
            cell: ({ getValue }) => <>{formatDistance(getValue<Date>(), new Date(), { addSuffix: true })}</>,
        }
    ], []);

    return (
        <div>
            <Typography variant="lead">Recent Trades</Typography>
            <ResponsiveDataTable
                columns={columns}
                data={data?.allTrades?.nodes}
                paginationArgs={paginationArgs}
                onPaginationArgsChanged={setPaginationArgs}
                disablePagination
                className="my-2"
            />
        </div>
    );
}