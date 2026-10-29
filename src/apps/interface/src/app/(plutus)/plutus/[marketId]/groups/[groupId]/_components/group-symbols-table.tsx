"use client";

import { PrettyNumber } from "@/components/shared/pretty-number";
import { type ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import { ResponsiveDropdownMenu } from "@/components/shared/responsive-dropdown-menu/responsive-dropdown-menu";
import { Button } from "@/components/ui/button";
import { type SymbolGroupSymbol } from "@/lib/api/plutus";
import { MoreHorizontal, Trash2 } from "lucide-react";
import Link from "next/link";
import { useMemo } from "react";

type GroupSymbolsTableProps = {
  marketId: string;
  symbols: SymbolGroupSymbol[];
  onRemoveSymbol: (symbolId: string) => void;
};

export function GroupSymbolsTable({
  marketId,
  symbols,
  onRemoveSymbol,
}: GroupSymbolsTableProps) {
  const columns: ExtendedColumnDef<SymbolGroupSymbol>[] = useMemo(
    () => [
      {
        id: "name",
        header: "Symbol",
        accessorFn: (row) => row.name,
        cell: ({ row, getValue }) => (
          <Link
            href={`/plutus/${marketId}/${row.original.symbolId}`}
            className="hover:underline"
          >
            {getValue<string>()}
            {row.original.subcode && (
              <span className="text-muted-foreground ml-2 text-sm">
                {row.original.subcode}
              </span>
            )}
          </Link>
        ),
      },
      {
        id: "volume",
        header: "Volume",
        accessorFn: (row) => row.volume,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? <PrettyNumber number={val} /> : "-";
        },
      },
      {
        id: "gain",
        header: "Gain",
        accessorFn: (row) => row.gain,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? <PrettyNumber number={val} /> : "-";
        },
      },
      {
        id: "roi",
        header: "ROI",
        accessorFn: (row) => row.roi,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? `${(val * 100).toFixed(2)}%` : "-";
        },
      },
      {
        id: "signalScore",
        header: "Signal",
        accessorFn: (row) => row.signalScore,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? val.toFixed(3) : "-";
        },
      },
      {
        id: "actions",
        header: "",
        cell: ({ row }) => (
          <ResponsiveDropdownMenu
            title="Symbol Actions"
            description="Available actions for this symbol"
            actions={[
              {
                label: "Remove",
                icon: <Trash2 className="h-4 w-4 text-destructive" />,
                onClick: () => onRemoveSymbol(row.original.symbolId),
              },
            ]}
          >
            <div className="border rounded-md">
              <Button variant="ghost" className="w-full">
                <MoreHorizontal className="m-auto" />
              </Button>
            </div>
          </ResponsiveDropdownMenu>
        ),
      },
    ],
    [marketId, onRemoveSymbol],
  );

  return (
    <ResponsiveDataTable
      columns={columns}
      data={symbols}
      scrollTop={false}
      disablePagination
      disableFiltering
      disableSorting
      className="my-2"
    />
  );
}
