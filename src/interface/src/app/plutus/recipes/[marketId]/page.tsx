"use client";

import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef, PaginationArgs, SortArgs } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/constants/plutus_store";
import { GET_RECIPE_TRADES } from "@/app/plutus/queries";
import { GetRecipeTradesResponse, GetRecipeTradesResponseFilterInput, SortEnumType } from "@/gql/graphql";
import { useQuery } from "@urql/next";
import { RefreshCw } from "lucide-react";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";

export default function RecentMarketTrades() {
    const { marketId } = useParams<{ marketId: string }>();
    const [timeFrameSeconds, setTimeFrameSeconds] = usePlutusStore((state: PlutusState) => [state.timeFrameSeconds, state.setTimeFrameSeconds]);
    const [paginationArgs, setPaginationArgs] = useState<PaginationArgs>({ pageSize: 10 });
    const [filter, setFilter] = useState<GetRecipeTradesResponseFilterInput>({});
    const [sort, setSort] = useState<SortArgs>({ averageMargin: SortEnumType.Desc });

    const [{ data, fetching }, reexecute] = useQuery({
        query: GET_RECIPE_TRADES,
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

    const columns: ExtendedColumnDef<GetRecipeTradesResponse>[] = useMemo(() => [
        {
            id: "recipeName",
            header: "Name",
            accessorFn: (row) => row.recipeName,
            cell: ({ getValue }) => getValue<string>(),
            filterConfig: {
                type: "string",
                operators: ["eq", "neq", "contains", "startsWith", "endsWith"],
            },
        },
        {
            id: "latestBuyPrice",
            header: "Latest Buy Price",
            accessorFn: (row) => row.latestBuyPrice,
            cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
            filterConfig: {
                type: "number",
                operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
            },
        },
        {
            id: "latestSellPrice",
            header: "Latest Sell Price",
            accessorFn: (row) => row.latestSellPrice,
            cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
            filterConfig: {
                type: "number",
                operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
            },
        },
        {
            id: "latestMargin",
            header: "Latest Margin",
            accessorFn: (row) => row.latestMargin,
            cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
            filterConfig: {
                type: "number",
                operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
            },
        },
        {
            id: "averageBuyPrice",
            header: "Average Buy Price",
            accessorFn: (row) => row.averageBuyPrice,
            cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
            filterConfig: {
                type: "number",
                operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
            },
        },
        {
            id: "averageSellPrice",
            header: "Average Sell Price",
            accessorFn: (row) => row.averageSellPrice,
            cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
            filterConfig: {
                type: "number",
                operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
            },
        },
        {
            id: "averageMargin",
            header: "Average Margin",
            accessorFn: (row) => row.averageMargin,
            cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
            filterConfig: {
                type: "number",
                operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
            },
        }
    ], []);

    return (
        <div>
            <div className="flex items-center gap-2 justify-between">
                <div className="flex items-end gap-2">
                    <Typography variant="lead">Recipes</Typography>
                </div>
                <div className="flex items-center gap-4">
                    {fetching ? <RefreshCw className="animate-spin" /> : <RefreshCw onClick={reexecute} className="hover:cursor-pointer" />}
                    <TimeFrameSelection onValueChange={setTimeFrameSeconds} seconds={timeFrameSeconds} />
                </div>
            </div>

            <ResponsiveDataTable
                columns={columns}
                data={data?.recipeTrades?.nodes}
                paginationArgs={paginationArgs}
                onPaginationArgsChanged={setPaginationArgs}
                pageInfo={data?.recipeTrades?.pageInfo}
                filter={filter}
                onFilterChange={setFilter}
                sort={sort}
                onSortChange={setSort}
                className="my-2"
            />
        </div>
    );
}