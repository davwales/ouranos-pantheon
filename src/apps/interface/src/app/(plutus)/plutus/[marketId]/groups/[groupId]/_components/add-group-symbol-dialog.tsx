"use client";

import {
  type SelectedSymbol,
  SymbolSearch,
} from "@/app/(plutus)/plutus/components/symbol-search";
import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { useState } from "react";

type AddGroupSymbolDialogProps = {
  marketId: string;
  excludeIds: string[];
  onAdd: (symbols: SelectedSymbol[]) => void;
};

export function AddGroupSymbolDialog({
  marketId,
  excludeIds,
  onAdd,
}: AddGroupSymbolDialogProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedSymbols, setSelectedSymbols] = useState<SelectedSymbol[]>([]);

  const handleAdd = () => {
    if (selectedSymbols.length === 0) return;
    onAdd(selectedSymbols);
    setIsOpen(false);
    setSelectedSymbols([]);
  };

  return (
    <ResponsiveDialog
      title="Add Symbol"
      description="Search for a symbol to add to this group"
      open={isOpen}
      onOpenChange={setIsOpen}
      trigger={
        <Button variant="ghost">
          <Plus className="w-4 h-4 mr-1" />
          Add Symbol
        </Button>
      }
    >
      <div>
        <SymbolSearch
          marketId={marketId}
          excludeIds={excludeIds}
          onSymbolsChanged={setSelectedSymbols}
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
  );
}
