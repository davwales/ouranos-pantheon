"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Typography } from "@/app/components/typography";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { GET_MARKET_FORECAST } from "@/app/plutus/queries";
import { useQuery } from "@urql/next";
import { formatDistance } from "date-fns";
import { RefreshCw } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo } from "react";

type ForecastRowData = {
  id: string;
  symbolId: string;
  symbolName: string;
  symbolSubcode?: string | null;
  latest: {
    averagePrice: any;
  };
  dayOne: {
    averagePrice: any;
    margin: any;
    gain: any;
  };
  dayTwo: {
    averagePrice: any;
    margin: any;
    gain: any;
  };
};

export default function RecentMarketTrades() {
  const { marketId } = useParams<{ marketId: string }>();
  const [tableState, setTableState] = usePlutusStore((state: PlutusState) => [
    state.forecastsTableState,
    state.setForecastsTableState,
  ]);

  const [{ data, fetching }, reexecute] = useQuery({
    query: GET_MARKET_FORECAST,
    variables: {
      marketId: marketId,
      order: tableState.sort,
      where: tableState.filter,
      first: tableState.pagination?.first,
      after: tableState.pagination?.after,
      last: tableState.pagination?.last,
      before: tableState.pagination?.before,
    },
  });

  const columns: ExtendedColumnDef<ForecastRowData>[] = useMemo(
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
        accessorFn: (x) => x.symbolSubcode,
        filterConfig: {
          type: "string",
          operators: ["eq", "neq", "contains", "startsWith", "endsWith"],
        },
      },
      {
        id: "latest.averagePrice",
        header: "Yesterday's Price",
        accessorFn: (x) => x.latest.averagePrice,
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
        id: "dayOne.averagePrice",
        header: "Today's Price",
        accessorFn: (x) => x.dayOne.averagePrice,
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
        id: "dayOne.margin",
        header: "Today's Margin",
        accessorFn: (x) => x.dayOne.margin,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "dayOne.gain",
        header: "Today's Gain",
        accessorFn: (x) => x.dayOne.gain,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "dayTwo.averagePrice",
        header: "Tomorrow's Price",
        accessorFn: (x) => x.dayTwo.averagePrice,
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
        id: "dayTwo.margin",
        header: "Tomorrow's Margin",
        accessorFn: (x) => x.dayTwo.margin,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
      {
        id: "dayTwo.gain",
        header: "Tomorrow's Gain",
        accessorFn: (x) => x.dayTwo.gain,
        cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
        filterConfig: {
          type: "number",
          operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
        },
      },
    ],
    [marketId]
  );

  const timeToRefresh = (): string => {
    const now = new Date();
    const midnightUTC = new Date(
      Date.UTC(
        now.getUTCFullYear(),
        now.getUTCMonth(),
        now.getUTCDate() + 1,
        0,
        0,
        0,
        0
      )
    );

    return formatDistance(midnightUTC, now, { addSuffix: true });
  };

  return (
    <div>
      <div className="flex items-end gap-2 justify-between">
        <div className="flex items-end gap-2">
          <Typography variant="lead">Forecasts</Typography>
        </div>
        <div className="flex items-center gap-4">
          <Typography variant="small">
            Next forecasts generated {timeToRefresh()}
          </Typography>
          {fetching ? (
            <RefreshCw className="animate-spin" />
          ) : (
            <RefreshCw onClick={reexecute} className="hover:cursor-pointer" />
          )}
        </div>
      </div>

      <ResponsiveDataTable
        columns={columns}
        data={data?.marketForecast?.nodes}
        state={tableState}
        onStateChange={setTableState}
        pageInfo={data?.marketForecast?.pageInfo}
        className="my-2"
      />
    </div>
  );
}
