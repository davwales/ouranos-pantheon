import ClipboardCopy from "@/components/shared/clipboard-copy";
import { PrettyNumber } from "@/components/shared/pretty-number";
import { type ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import { type GetMarketForecastRow } from "@/lib/api/plutus";
import Link from "next/link";

export function makeMarketForecastColumns(
  marketId: string,
): ExtendedColumnDef<GetMarketForecastRow>[] {
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
        searchable: true,
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
  ];
}
