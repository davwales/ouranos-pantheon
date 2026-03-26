"use client";

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
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { GetRecipeTradesRow, plutusApi } from "@/lib/api/plutus";
import { Plus, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";

export default function RecipesPage() {
  const { marketId } = useParams<{ marketId: string }>();
  const [timeFrameKey, tableState, setTableState] = usePlutusStore(
    useShallow((state: PlutusState) => [
      state.timeFrameKey,
      state.recipesTableState,
      state.setRecipesTableState,
    ]),
  );

  const { sortField, sortDirection } = extractSort(tableState.sort);
  const filter = useMemo(
    () => extractFilter(tableState.filter),
    [tableState.filter],
  );

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getRecipeTrades(marketId, timeFrameKey, {
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

  const columns: ExtendedColumnDef<GetRecipeTradesRow>[] = useMemo(
    () => [
      {
        id: "recipeName",
        header: "Name",
        accessorFn: (row) => row.recipeName,
        cell: ({ cell, row }) => (
          <Link
            href={`/plutus/${marketId}/recipes/${row.original.recipeId}`}
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
      },
    ],
    [marketId],
  );

  return (
    <div>
      <div className="flex items-center gap-2 justify-between">
        <div className="flex items-end gap-2">
          <Typography variant="lead">Recipes</Typography>
        </div>
        <div className="flex items-center gap-4">
          <Link href={`/plutus/${marketId}/recipes/create`}>
            <Button variant="link" className="flex items-end gap-0">
              <Plus className="w-4 h-4 mr-1" />
              Create Recipe
            </Button>
          </Link>
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
        state={tableState}
        onStateChange={setTableState}
        pageInfo={pageInfo}
        className="my-2"
      />
    </div>
  );
}
