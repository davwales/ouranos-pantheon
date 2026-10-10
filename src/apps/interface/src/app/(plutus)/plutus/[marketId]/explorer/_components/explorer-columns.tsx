import ClipboardCopy from "@/components/shared/clipboard-copy";
import { PrettyNumber } from "@/components/shared/pretty-number";
import { type ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import { type GetMarketTradesRow } from "@/lib/api/plutus";
import Link from "next/link";

export function explorerColumns(
  marketId: string,
): ExtendedColumnDef<GetMarketTradesRow>[] {
  return [
    {
      id: "symbolName",
      header: "Name",
      accessorFn: (row) => row.symbolName,
      cell: ({ cell, row }) => (
        <Link
          href={`/plutus/${marketId}/${row.original.symbolId}`}
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
  ];
}
