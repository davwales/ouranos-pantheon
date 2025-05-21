"use client";

import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { GET_RECIPE_TRADES } from "@/app/plutus/queries";
import { Button } from "@/components/ui/button";
import { GetRecipeTradesResponse } from "@/gql/graphql";
import { useQuery } from "@urql/next";
import { Plus, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";

export default function RecipesPage() {
  const { marketId } = useParams<{ marketId: string }>();
  const [timeFrameSeconds, tableState, setTableState] = usePlutusStore(
    (state: PlutusState) => [
      state.timeFrameSeconds,
      state.recipesTableState,
      state.setRecipesTableState,
    ]
  );

  const [{ data, fetching }, reexecute] = useQuery({
    query: GET_RECIPE_TRADES,
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

  const columns: ExtendedColumnDef<GetRecipeTradesResponse>[] = useMemo(
    () => [
      {
        id: "recipeName",
        header: "Name",
        accessorFn: (row) => row.recipeName,
        cell: ({ cell, row }) => (
          <Link
            href={`/plutus/recipes/${marketId}/${row.original.recipeId}`}
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
    []
  );

  return (
    <div>
      <div className="flex items-center gap-2 justify-between">
        <div className="flex items-end gap-2">
          <Typography variant="lead">Recipes</Typography>
        </div>
        <div className="flex items-center gap-4">
          <Link href={`/plutus/recipes/${marketId}/create`}>
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
        data={data?.recipeTrades?.nodes}
        state={tableState}
        onStateChange={setTableState}
        pageInfo={data?.recipeTrades?.pageInfo}
        className="my-2"
      />
    </div>
  );
}
