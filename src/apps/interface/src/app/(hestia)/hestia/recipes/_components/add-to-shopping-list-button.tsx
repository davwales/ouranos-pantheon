"use client";

import { Button } from "@/components/ui/button";
import { Check, ListPlus } from "lucide-react";
import type { RecipeImportStatus } from "@/lib/api/hestia-types";

export function isShoppingListActionDisabled(
  importStatus: RecipeImportStatus,
): boolean {
  return importStatus === "Importing" || importStatus === "Failed";
}

export type AddToShoppingListButtonProps = {
  recipeId: string;
  isInList: boolean;
  onToggle: (recipeId: string) => void;
  disabled?: boolean;
};

export function AddToShoppingListButton({
  recipeId,
  isInList,
  onToggle,
  disabled,
}: AddToShoppingListButtonProps) {
  return (
    <Button
      variant={isInList ? "secondary" : "outline"}
      size="sm"
      onClick={() => onToggle(recipeId)}
      disabled={disabled}
    >
      {isInList ? <Check className="size-4" /> : <ListPlus className="size-4" />}
      {isInList ? "In Shopping List" : "Add to Shopping List"}
    </Button>
  );
}
