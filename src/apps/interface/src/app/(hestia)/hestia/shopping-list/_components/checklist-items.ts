import type { ConsolidatedIngredient, ShoppingListResponse } from "@/lib/api/hestia-types";
import type { ChecklistItem } from "./shopping-checklist";

export function manualLineId(itemId: string): string {
  return `manual:${itemId}`;
}

export function formatIngredient({ quantity, unit, name }: ConsolidatedIngredient): string {
  if (quantity <= 0) {
    return unit ? `${name} (${unit})` : name;
  }
  // Converted quantities can carry three decimals (1.333 cup); two read better on a list.
  const amount = Number(quantity.toFixed(2));
  return [amount, unit, name].filter((part) => part !== "").join(" ");
}

export function toChecklistItems(data: ShoppingListResponse): ChecklistItem[] {
  return [
    ...data.consolidatedIngredients.map((ingredient) => ({
      lineId: ingredient.id,
      label: formatIngredient(ingredient),
      name: ingredient.name,
      category: ingredient.category,
    })),
    ...data.manualItems.map((item) => ({
      lineId: manualLineId(item.id),
      label: item.text,
      name: item.text,
      category: item.category,
      manualItemId: item.id,
    })),
  ];
}
