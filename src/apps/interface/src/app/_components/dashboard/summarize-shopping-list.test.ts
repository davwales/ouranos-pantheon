import { describe, expect, it } from "vitest";
import { summarizeShoppingList } from "./summarize-shopping-list";

describe("summarizeShoppingList", () => {
  it("counts ingredients and manual items, excluding checked ones", () => {
    const summary = summarizeShoppingList({
      recipeIds: ["r1"],
      recipes: [{ id: "r1", title: "Soup" }],
      consolidatedIngredients: [
        { id: "i1", name: "Leek", unit: "", quantity: 2, category: null },
        { id: "i2", name: "Stock", unit: "l", quantity: 1, category: null },
      ],
      manualItems: [{ id: "m1", text: "Bread", category: null }],
      checkedItemIds: ["i2", "stale-id"],
    });

    expect(summary).toEqual({ totalItems: 3, remainingItems: 2 });
  });

  it("counts a checked manual item as done", () => {
    const summary = summarizeShoppingList({
      recipeIds: [],
      recipes: [],
      consolidatedIngredients: [],
      manualItems: [{ id: "m1", text: "Bread", category: null }],
      checkedItemIds: ["manual:m1"],
    });

    expect(summary).toEqual({ totalItems: 1, remainingItems: 0 });
  });

  it("returns zero for an empty list", () => {
    expect(
      summarizeShoppingList({
        recipeIds: [],
        recipes: [],
        consolidatedIngredients: [],
        manualItems: [],
        checkedItemIds: [],
      }),
    ).toEqual({ totalItems: 0, remainingItems: 0 });
  });
});
