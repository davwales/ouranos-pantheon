import ClipboardCopy from "@/components/shared/clipboard-copy";
import { PrettyNumber } from "@/components/shared/pretty-number/pretty-number";
import { type ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import { type GetSignalRankingsRow } from "@/lib/api/plutus";
import { OriginLink } from "@/components/shared/breadcrumbs";


function ScoreCell({ value }: { value: number | null | undefined }) {
  if (value == null) return <span className="text-muted-foreground">-</span>;
  const color =
    value > 0
      ? "text-green-600 dark:text-green-400"
      : value < 0
        ? "text-red-600 dark:text-red-400"
        : "text-muted-foreground";
  return <span className={color}>{value.toFixed(3)}</span>;
}

export function makeSignalRankingColumns(
  marketId: string,
): ExtendedColumnDef<GetSignalRankingsRow>[] {
  return [
    {
      id: "symbolName",
      header: "Name",
      accessorFn: (row) => row.symbolName,
      cell: ({ cell, row }) => (
        <OriginLink
          href={`/plutus/${marketId}/${row.original.symbolId}`}
          className="hover:underline"
        >
          {cell.getValue<string>()}
        </OriginLink>
      ),
      filterConfig: {
        type: "string",
        operators: ["eq", "neq", "contains", "startsWith", "endsWith"],
        searchable: true,
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
      id: "dailyAveragePrice",
      header: "Avg Price",
      accessorFn: (row) => row.dailyAveragePrice,
      cell: ({ getValue }) => {
        const v = getValue<number | null | undefined>();
        return v ? (
          <ClipboardCopy value={v}>
            <PrettyNumber number={v} />
          </ClipboardCopy>
        ) : (
          <span className="text-muted-foreground">-</span>
        );
      },
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "dailyVolume",
      header: "Volume",
      accessorFn: (row) => row.dailyVolume,
      cell: ({ getValue }) => {
        const v = getValue<number | null | undefined>();
        return v ? (
          <ClipboardCopy value={v}>
            <PrettyNumber number={v} />
          </ClipboardCopy>
        ) : (
          <span className="text-muted-foreground">-</span>
        );
      },
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "overallScore",
      header: "Overall",
      accessorFn: (row) => row.overallScore,
      cell: ({ getValue }) => <ScoreCell value={getValue<number>()} />,
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "buyScore",
      header: "Buy",
      accessorFn: (row) => row.buyScore,
      cell: ({ getValue }) => (
        <ScoreCell value={getValue<number | null | undefined>()} />
      ),
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "sellScore",
      header: "Sell",
      accessorFn: (row) => row.sellScore,
      cell: ({ getValue }) => (
        <ScoreCell value={getValue<number | null | undefined>()} />
      ),
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "flipScore",
      header: "Flip",
      accessorFn: (row) => row.flipScore,
      cell: ({ getValue }) => (
        <ScoreCell value={getValue<number | null | undefined>()} />
      ),
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "merchScore",
      header: "Merch",
      accessorFn: (row) => row.merchScore,
      cell: ({ getValue }) => (
        <ScoreCell value={getValue<number | null | undefined>()} />
      ),
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "signalCount",
      header: "Signals",
      accessorFn: (row) => row.signalCount,
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "bullishCount",
      header: "Bullish",
      accessorFn: (row) => row.bullishCount,
      cell: ({ getValue }) => (
        <span className="text-green-600">
          {getValue<number>()}
        </span>
      ),
    },
    {
      id: "bearishCount",
      header: "Bearish",
      accessorFn: (row) => row.bearishCount,
      cell: ({ getValue }) => (
        <span className="text-red-600">
          {getValue<number>()}
        </span>
      ),
    },
  ];
}
