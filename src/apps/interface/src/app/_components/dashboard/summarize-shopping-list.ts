import { type ShoppingListResponse } from "@/lib/api/hestia-types";

export type ShoppingListSummary = {
  totalItems: number;
  remainingItems: number;
};

export function summarizeShoppingList(
  list: ShoppingListResponse,
): ShoppingListSummary {
  const itemIds = [
    ...list.consolidatedIngredients.map((i) => i.id),
    ...list.manualItems.map((i) => i.id),
  ];
  const checked = new Set(list.checkedItemIds);
  return {
    totalItems: itemIds.length,
    remainingItems: itemIds.filter((id) => !checked.has(id)).length,
  };
}
