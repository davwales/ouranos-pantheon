import { PrettyNumber } from "@/components/shared/pretty-number";
import { type ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import {
  type BacktestKind,
  type BacktestStatus,
  type BacktestSummary,
} from "@/lib/api/plutus";
import Link from "next/link";
import { StatusChip } from "./status-chip";

export function makeBacktestColumns(
  marketId: string,
  strategyId: string,
): ExtendedColumnDef<BacktestSummary>[] {
  return [
    {
      id: "status",
      header: "Status",
      accessorFn: (row) => row.status,
      cell: ({ row, getValue }) => (
        <Link
          href={`/plutus/${marketId}/strategies/${strategyId}/backtests/${row.original.id}`}
          className="hover:underline"
        >
          <StatusChip status={getValue<BacktestStatus>()} />
        </Link>
      ),
    },
    {
      id: "kind",
      header: "Kind",
      accessorFn: (row) => row.kind,
      cell: ({ row, getValue }) => {
        const kind = getValue<BacktestKind>();
        return (
          <Link
            href={`/plutus/${marketId}/strategies/${strategyId}/backtests/${row.original.id}`}
            className="hover:underline"
          >
            <span
              className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                kind === "Optimization"
                  ? "bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400"
                  : "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-400"
              }`}
            >
              {kind}
            </span>
          </Link>
        );
      },
    },
    {
      id: "startDate",
      header: "Start Date",
      accessorFn: (row) => row.startDate,
      cell: ({ getValue }) =>
        new Date(getValue<string>()).toLocaleDateString(),
    },
    {
      id: "endDate",
      header: "End Date",
      accessorFn: (row) => row.endDate,
      cell: ({ getValue }) =>
        new Date(getValue<string>()).toLocaleDateString(),
    },
    {
      id: "budget",
      header: "Budget",
      accessorFn: (row) => row.budget,
      cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
    },
    {
      id: "totalReturnPercent",
      header: "Return %",
      accessorFn: (row) => row.totalReturnPercent,
      cell: ({ getValue }) => {
        const val = getValue<number | null>();
        if (val == null) return "-";
        const color = val >= 0 ? "text-green-600" : "text-red-600";
        return <span className={color}>{(val * 100).toFixed(2)}%</span>;
      },
    },
    {
      id: "winRate",
      header: "Win Rate",
      accessorFn: (row) => row.winRate,
      cell: ({ getValue }) => {
        const val = getValue<number | null>();
        return val != null ? `${(val * 100).toFixed(1)}%` : "-";
      },
    },
    {
      id: "sharpeRatio",
      header: "Sharpe",
      accessorFn: (row) => row.sharpeRatio,
      cell: ({ getValue }) => {
        const val = getValue<number | null>();
        return val != null ? val.toFixed(2) : "-";
      },
    },
    {
      id: "totalTrades",
      header: "Trades",
      accessorFn: (row) => row.totalTrades,
      cell: ({ getValue }) => getValue<number | null>() ?? "-",
    },
    {
      id: "createdAt",
      header: "Created",
      accessorFn: (row) => row.createdAt,
      cell: ({ getValue }) =>
        new Date(getValue<string>()).toLocaleDateString(),
    },
  ];
}
