import { PrettyNumber } from "@/components/shared/pretty-number";
import { type ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import { type SymbolGroup } from "@/lib/api/plutus";
import Link from "next/link";

export function makeSymbolGroupColumns(
  marketId: string,
): ExtendedColumnDef<SymbolGroup>[] {
  return [
    {
      id: "name",
      header: "Name",
      accessorFn: (row) => row.name,
      cell: ({ cell, row }) => (
        <Link
          href={`/plutus/${marketId}/groups/${row.original.id}`}
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
      id: "symbolCount",
      header: "Symbols",
      accessorFn: (row) => row.symbolCount,
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "totalVolume",
      header: "Total Volume",
      accessorFn: (row) => row.totalVolume,
      cell: ({ getValue }) => {
        const val = getValue<number | null>();
        return val != null ? <PrettyNumber number={val} /> : "-";
      },
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "totalGain",
      header: "Total Gain",
      accessorFn: (row) => row.totalGain,
      cell: ({ getValue }) => {
        const val = getValue<number | null>();
        return val != null ? <PrettyNumber number={val} /> : "-";
      },
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "averageRoi",
      header: "Avg ROI",
      accessorFn: (row) => row.averageRoi,
      cell: ({ getValue }) => {
        const val = getValue<number | null>();
        return val != null ? `${(val * 100).toFixed(2)}%` : "-";
      },
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "averageOverallScore",
      header: "Avg Signal",
      accessorFn: (row) => row.averageOverallScore,
      cell: ({ getValue }) => {
        const val = getValue<number | null>();
        return val != null ? val.toFixed(3) : "-";
      },
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "bullishCount",
      header: "Bullish",
      accessorFn: (row) => row.bullishCount,
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "bearishCount",
      header: "Bearish",
      accessorFn: (row) => row.bearishCount,
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
  ];
}
