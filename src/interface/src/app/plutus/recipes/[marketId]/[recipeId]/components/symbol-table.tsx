import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { ResponsiveDialog } from "@/app/components/responsive-dialog";
import { ResponsiveDropdownMenu } from "@/app/components/responsive-dropdown-menu/responsive-dropdown-menu";
import { SelectedSymbol, SymbolSearch } from "@/app/plutus/components/symbol-search";
import { RecipeSymbol } from "@/app/plutus/recipes/[marketId]/[recipeId]/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { MoreHorizontal, Plus, Trash2 } from "lucide-react";

export function SymbolTable({
    title,
    items,
    onAdd,
    onRemove,
    onQuantityChange,
    isDialogOpen,
    onDialogOpenChange,
    selectedSymbol,
    onSymbolSelected,
    onAddSymbol
}: {
    title: string;
    items: RecipeSymbol[];
    onAdd: () => void;
    onRemove: (index: number) => void;
    onQuantityChange: (index: number, quantity: number) => void;
    isDialogOpen: boolean;
    onDialogOpenChange: (open: boolean) => void;
    selectedSymbol?: SelectedSymbol;
    onSymbolSelected: (symbol: SelectedSymbol) => void;
    onAddSymbol: () => void;
}) {
    const columns: ExtendedColumnDef<RecipeSymbol>[] = [
        {
            id: "name",
            header: "Symbol",
            accessorFn: (row: RecipeSymbol) => row.name,
            cell: ({ getValue }) => getValue() as string
        },
        {
            id: "quantity",
            header: "Quantity",
            accessorFn: (row: RecipeSymbol) => row.quantity,
            cell: ({ row, getValue }) => (
                <Input
                    value={getValue() as number}
                    onChange={(e) => onQuantityChange(row.index, Number(e.target.value))}
                    min={1}
                    type="number"
                />
            )
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
                            onClick: () => onRemove(row.index)
                        }
                    ]}
                >
                    <div className="border-1 rounded-md">
                        <Button variant="ghost" className="w-full">
                            <MoreHorizontal className="m-auto" />
                        </Button>
                    </div>
                </ResponsiveDropdownMenu>
            )
        }
    ];

    return (
        <div>
            <div className="flex items-center justify-between mb-2">
                <h3 className="text-lg font-medium">{title}</h3>

                <ResponsiveDialog
                    title={`Add ${title.slice(0, -1)}`}
                    description={`Add an ${title.slice(0, -1).toLowerCase()} to the recipe`}
                    open={isDialogOpen}
                    onOpenChange={onDialogOpenChange}
                    trigger={(
                        <Button variant="ghost">
                            <Plus className="w-4 h-4 mr-1" />
                            Add {title.slice(0, -1)}
                        </Button>
                    )}
                >
                    <div>
                        <SymbolSearch
                            onSymbolSelected={onSymbolSelected}
                        />

                        <Button
                            onClick={onAddSymbol}
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
};