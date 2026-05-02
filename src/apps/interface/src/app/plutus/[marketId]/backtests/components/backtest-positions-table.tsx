"use client";

import { type ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { BacktestPosition } from "@/lib/api/plutus";
import { useMemo } from "react";

export function BacktestPositionsTable({
  positions,
}: {
  positions: BacktestPosition[];
}) {
  const columns = useMemo<ExtendedColumnDef<BacktestPosition>[]>(
    () => [
      {
        id: "symbolName",
        header: "Symbol",
        accessorFn: (row) => row.symbolName,
      },
      {
        id: "entryPrice",
        header: "Entry Price",
        accessorFn: (row) => row.entryPrice,
        cell: ({ getValue }) => (getValue() as number).toLocaleString(),
      },
      {
        id: "exitPrice",
        header: "Exit Price",
        accessorFn: (row) => row.exitPrice,
        cell: ({ getValue }) => (getValue() as number).toLocaleString(),
      },
      {
        id: "volume",
        header: "Volume",
        accessorFn: (row) => row.volume,
        cell: ({ getValue }) => (getValue() as number).toLocaleString(),
      },
      {
        id: "profitLoss",
        header: "P&L",
        accessorFn: (row) => row.profitLoss,
        cell: ({ getValue }) => {
          const val = getValue() as number;
          const color =
            val >= 0
              ? "text-green-600 dark:text-green-400"
              : "text-red-600 dark:text-red-400";
          return <span className={color}>{val.toLocaleString()}</span>;
        },
      },
      {
        id: "returnPercent",
        header: "Return %",
        accessorFn: (row) => row.returnPercent,
        cell: ({ getValue }) => {
          const val = getValue() as number;
          const color =
            val >= 0
              ? "text-green-600 dark:text-green-400"
              : "text-red-600 dark:text-red-400";
          return <span className={color}>{(val * 100).toFixed(2)}%</span>;
        },
      },
      {
        id: "entryTime",
        header: "Entry Time",
        accessorFn: (row) => row.entryTime,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleString(),
      },
      {
        id: "exitTime",
        header: "Exit Time",
        accessorFn: (row) => row.exitTime,
        cell: ({ getValue }) => new Date(getValue() as string).toLocaleString(),
      },
    ],
    [],
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Positions ({positions.length})</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveDataTable
          columns={columns}
          data={positions}
          disablePagination
          disableSorting
          disableFiltering
        />
      </CardContent>
    </Card>
  );
}
