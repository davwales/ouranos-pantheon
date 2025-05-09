"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef, PaginationArgs, SortArgs } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/constants/plutus_store";
import { GET_MARKET_TRADES } from "@/app/plutus/queries";
import { GetMarketTradesResponse, GetMarketTradesResponseFilterInput, SortEnumType } from "@/gql/graphql";
import { useQuery } from "@urql/next";
import { RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";

export default function MarketDetail() {
    const { marketId } = useParams<{ marketId: string }>();
    const [timeFrameSeconds, setTimeFrameSeconds] = usePlutusStore((state: PlutusState) => [state.timeFrameSeconds, state.setTimeFrameSeconds]);
    const [paginationArgs, setPaginationArgs] = useState<PaginationArgs>({ pageSize: 10 });
    const [filter, setFilter] = useState<GetMarketTradesResponseFilterInput>({});
    const [sort, setSort] = useState<SortArgs>({ totalGain: SortEnumType.Desc });

    const [{ data, fetching }, reexecute] = useQuery({
        query: GET_MARKET_TRADES,
        variables: {
            input: {
                marketId: marketId,
                seconds: timeFrameSeconds > 0 ? timeFrameSeconds : undefined
            },
            order: sort,
            where: filter,
            first: paginationArgs.first,
            after: paginationArgs.after,
            last: paginationArgs.last,
            before: paginationArgs.before
        }
    });

    const columns: ExtendedColumnDef<GetMarketTradesResponse>[] = useMemo(() => [
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
    ], []);

    return (
        <div>
            <div className="flex items-center gap-2 justify-between">
                <div className="flex items-end gap-2">
                    <Typography variant="lead">Explorer</Typography>
                </div>
                <div className="flex items-center gap-4">
                    {fetching ? <RefreshCw className="animate-spin" /> : <RefreshCw onClick={reexecute} className="hover:cursor-pointer" />}
                    <TimeFrameSelection onValueChange={setTimeFrameSeconds} seconds={timeFrameSeconds} />
                </div>
            </div>

            <ResponsiveDataTable
                columns={columns}
                data={data?.marketTrades?.nodes}
                paginationArgs={paginationArgs}
                onPaginationArgsChanged={setPaginationArgs}
                pageInfo={data?.marketTrades?.pageInfo}
                filter={filter}
                onFilterChange={setFilter}
                sort={sort}
                onSortChange={setSort}
                className="my-2"
            />
        </div>
    );
}