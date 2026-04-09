import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { ResponsiveDialog } from "@/app/components/responsive-dialog";
import { ResponsiveDropdownMenu } from "@/app/components/responsive-dropdown-menu/responsive-dropdown-menu";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { MoreHorizontal, Plus, Trash2 } from "lucide-react";
import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import {
  SelectedSymbol,
  SymbolSearch,
} from "../../../components/symbol-search";

interface TableSymbol {
  name: string;
  quantity: number;
  symbolId: string;
  latestPrice?: number | null;
  averagePrice?: number | null;
  totalValue?: number | null;
  volume?: number | null;
}

function QuantityInput({
  initialValue,
  onValueCommit,
}: {
  initialValue: number;
  onValueCommit: (value: number) => void;
}) {
  const [value, setValue] = useState(initialValue);

  useEffect(() => {
    setValue(initialValue);
  }, [initialValue]);

  const handleBlur = () => {
    if (value !== initialValue) {
      onValueCommit(value);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.currentTarget.blur();
    }
  };

  return (
    <Input
      value={value}
      onChange={(e) => setValue(Number(e.target.value))}
      onBlur={handleBlur}
      onKeyDown={handleKeyDown}
      min={1}
      type="number"
    />
  );
}

export function SymbolTable({
  marketId,
  title,
  items,
  onItemsChange,
  isDialogOpen,
  onDialogOpenChange,
  selectedSymbols,
  onSymbolsChanged,
}: {
  marketId: string;
  title: string;
  items: TableSymbol[];
  onItemsChange: (items: TableSymbol[]) => void;
  isDialogOpen: boolean;
  onDialogOpenChange: (open: boolean) => void;
  selectedSymbols: SelectedSymbol[];
  onSymbolsChanged: (symbols: SelectedSymbol[]) => void;
}) {
  const handleAdd = () => {
    if (selectedSymbols.length > 0) {
      const newItems = [
        ...items,
        ...selectedSymbols.map((s) => ({
          name: s.name,
          quantity: 1,
          symbolId: s.id,
        })),
      ];
      onItemsChange(newItems);
      onDialogOpenChange(false);
      onSymbolsChanged([]);
    }
  };

  const handleQuantityChange = useCallback(
    (item: TableSymbol, quantity: number) => {
      const newItems = items.map((i) =>
        i.symbolId === item.symbolId ? { ...i, quantity } : i,
      );
      onItemsChange(newItems);
    },
    [items, onItemsChange],
  );

  const handleRemove = useCallback(
    (item: TableSymbol) => {
      const newItems = items.filter(
        (i: TableSymbol) => i.symbolId !== item.symbolId,
      );
      onItemsChange(newItems);
    },
    [items, onItemsChange],
  );

  const columns: ExtendedColumnDef<TableSymbol>[] = useMemo(
    () => [
      {
        id: "name",
        header: "Symbol",
        accessorFn: (row: TableSymbol) => row.name,
        cell: ({ row, getValue }) => (
          <Link
            href={`/plutus/${marketId}/${row.original.symbolId}`}
            className="hover:underline"
          >
            {getValue<string>()}
          </Link>
        ),
      },
      {
        id: "quantity",
        header: "Quantity",
        accessorFn: (row: TableSymbol) => row.quantity,
        cell: ({ row }) => (
          <QuantityInput
            initialValue={row.original.quantity}
            onValueCommit={(quantity) =>
              handleQuantityChange(row.original, quantity)
            }
          />
        ),
      },
      {
        id: "latestPrice",
        header: "Latest Price",
        accessorFn: (row: TableSymbol) => row.latestPrice,
        cell: ({ row }) =>
          row.original.latestPrice != null ? (
            <PrettyNumber number={row.original.latestPrice} />
          ) : (
            "-"
          ),
      },
      {
        id: "totalValue",
        header: "Total Value",
        accessorFn: (row: TableSymbol) => row.totalValue,
        cell: ({ row }) =>
          row.original.totalValue != null ? (
            <PrettyNumber number={row.original.totalValue} />
          ) : (
            "-"
          ),
      },
      {
        id: "volume",
        header: "Volume",
        accessorFn: (row: TableSymbol) => row.volume,
        cell: ({ row }) =>
          row.original.volume != null ? (
            <PrettyNumber number={row.original.volume} />
          ) : (
            "-"
          ),
      },
      {
        id: "actions",
        header: "",
        cell: ({ row }) => (
          <ResponsiveDropdownMenu
            title={`${title} Actions`}
            description={`Available actions for this ${title.toLowerCase()}`}
            actions={[
              {
                label: "Remove",
                icon: <Trash2 className="h-4 w-4 text-destructive" />,
                onClick: () => handleRemove(row.original),
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
    [marketId, title, handleQuantityChange, handleRemove],
  );

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <h3 className="text-lg font-medium">{title}</h3>

        <ResponsiveDialog
          title={`Add ${title.slice(0, -1)}`}
          description={`Add an ${title
            .slice(0, -1)
            .toLowerCase()} to the recipe`}
          open={isDialogOpen}
          onOpenChange={onDialogOpenChange}
          trigger={
            <Button variant="ghost">
              <Plus className="w-4 h-4 mr-1" />
              Add {title.slice(0, -1)}
            </Button>
          }
        >
          <div>
            <SymbolSearch
              marketId={marketId}
              onSymbolsChanged={onSymbolsChanged}
            />

            <Button
              onClick={handleAdd}
              disabled={selectedSymbols.length === 0}
              className="mt-2 w-full"
            >
              {selectedSymbols.length > 0
                ? `Add (${selectedSymbols.length})`
                : "Add"}
            </Button>
          </div>
        </ResponsiveDialog>
      </div>

      <ResponsiveDataTable
        columns={columns}
        data={items}
        scrollTop={false}
        disablePagination
        disableFiltering
        disableSorting
        className="my-2"
      />
    </div>
  );
}
