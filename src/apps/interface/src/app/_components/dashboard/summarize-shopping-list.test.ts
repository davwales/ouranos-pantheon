import { describe, expect, it } from "vitest";
import { summarizeShoppingList } from "./summarize-shopping-list";

describe("summarizeShoppingList", () => {
  it("counts ingredients and manual items, excluding checked ones", () => {
    const summary = summarizeShoppingList({
      recipeIds: ["r1"],
      recipes: [{ id: "r1", title: "Soup" }],
      consolidatedIngredients: [
        { id: "i1", name: "Leek", unit: "", quantity: 2 },
        { id: "i2", name: "Stock", unit: "l", quantity: 1 },
      ],
      manualItems: [{ id: "m1", text: "Bread" }],
      checkedItemIds: ["i2", "stale-id"],
    });

    expect(summary).toEqual({ totalItems: 3, remainingItems: 2 });
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
