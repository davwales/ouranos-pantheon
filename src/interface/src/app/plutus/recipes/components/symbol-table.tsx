import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { ResponsiveDialog } from "@/app/components/responsive-dialog";
import { ResponsiveDropdownMenu } from "@/app/components/responsive-dropdown-menu/responsive-dropdown-menu";
import {
  SelectedSymbol,
  SymbolSearch,
} from "@/app/plutus/components/symbol-search";
import { RecipeSymbol } from "@/app/plutus/recipes/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { MoreHorizontal, Plus, Trash2 } from "lucide-react";
import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";

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
  selectedSymbol,
  onSymbolSelected,
}: {
  marketId: string;
  title: string;
  items: RecipeSymbol[];
  onItemsChange: (items: RecipeSymbol[]) => void;
  isDialogOpen: boolean;
  onDialogOpenChange: (open: boolean) => void;
  selectedSymbol?: SelectedSymbol;
  onSymbolSelected: (symbol: SelectedSymbol) => void;
}) {
  const handleAdd = () => {
    if (selectedSymbol) {
      const newItems = [
        ...items,
        { name: selectedSymbol.name, quantity: 1, symbolId: selectedSymbol.id },
      ];
      onItemsChange(newItems);
      onDialogOpenChange(false);
      onSymbolSelected(null as unknown as SelectedSymbol);
    }
  };

  const handleQuantityChange = useCallback(
    (item: RecipeSymbol, quantity: number) => {
      const newItems = items.map((i) =>
        i.symbolId === item.symbolId ? { ...i, quantity } : i
      );
      onItemsChange(newItems);
    },
    [items, onItemsChange]
  );

  const handleRemove = useCallback(
    (item: RecipeSymbol) => {
      const newItems = items.filter(
        (i: RecipeSymbol) => i.symbolId !== item.symbolId
      );
      onItemsChange(newItems);
    },
    [items, onItemsChange]
  );

  const columns: ExtendedColumnDef<RecipeSymbol>[] = useMemo(
    () => [
      {
        id: "name",
        header: "Symbol",
        accessorFn: (row: RecipeSymbol) => row.name,
        cell: ({ row, getValue }) => (
          <Link
            href={`/plutus/explorer/${marketId}/${row.original.symbolId}`}
            className="hover:underline"
          >
            {getValue<string>()}
          </Link>
        ),
      },
      {
        id: "quantity",
        header: "Quantity",
        accessorFn: (row: RecipeSymbol) => row.quantity,
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
            <div className="border-1 rounded-md">
              <Button variant="ghost" className="w-full">
                <MoreHorizontal className="m-auto" />
              </Button>
            </div>
          </ResponsiveDropdownMenu>
        ),
      },
    ],
    [marketId, title, handleQuantityChange, handleRemove]
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
              onSymbolSelected={onSymbolSelected}
            />

            <Button
              onClick={handleAdd}
              disabled={!selectedSymbol}
              className="mt-2 w-full"
            >
              Add
            </Button>
          </div>
        </ResponsiveDialog>
      </div>

      <ResponsiveDataTable
        columns={columns}
        data={items}
        scrollTop={false}
        disablePagination
        className="my-2"
      />
    </div>
  );
}
