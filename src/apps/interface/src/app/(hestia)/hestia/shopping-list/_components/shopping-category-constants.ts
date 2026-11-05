import type { ShoppingCategory } from "@/lib/api/hestia-types";

// Store-walk order; the list groups in this order.
export const SHOPPING_CATEGORIES: { value: ShoppingCategory; label: string }[] = [
  { value: "Produce", label: "Produce" },
  { value: "Bakery", label: "Bakery" },
  { value: "MeatAndSeafood", label: "Meat & Seafood" },
  { value: "DairyAndEggs", label: "Dairy & Eggs" },
  { value: "Frozen", label: "Frozen" },
  { value: "Pantry", label: "Pantry" },
  { value: "SpicesAndSeasonings", label: "Spices & Seasonings" },
  { value: "Beverages", label: "Beverages" },
  { value: "Household", label: "Household" },
];

export const UNCATEGORIZED_LABEL = "Uncategorized";
