import { describe, expect, it } from "vitest";
import { formatIngredient, toChecklistItems } from "./checklist-items";

describe("formatIngredient", () => {
  it("renders quantity, unit, and name", () => {
    expect(
      formatIngredient({ id: "l", name: "flour", unit: "cup", quantity: 2, category: null }),
    ).toBe("2 cup flour");
  });

  it("rounds converted quantities to two decimals", () => {
    expect(
      formatIngredient({ id: "l", name: "sugar", unit: "tbsp", quantity: 1.333, category: null }),
    ).toBe("1.33 tbsp sugar");
  });

  it("omits an empty unit without extra whitespace", () => {
    expect(
      formatIngredient({ id: "l", name: "eggs", unit: "", quantity: 3, category: null }),
    ).toBe("3 eggs");
  });

  it("shows an unquantified line as the name with its note", () => {
    expect(
      formatIngredient({ id: "l", name: "salt", unit: "to taste", quantity: 0, category: null }),
    ).toBe("salt (to taste)");
  });
});

describe("toChecklistItems", () => {
  it("maps ingredients and manual items to checklist rows", () => {
    const items = toChecklistItems({
      recipeIds: [],
      recipes: [],
      consolidatedIngredients: [
        { id: "recipe:egg|whole", name: "Eggs", unit: "whole", quantity: 2, category: "DairyAndEggs" },
      ],
      manualItems: [{ id: "m1", text: "Foil", category: null }],
      checkedItemIds: [],
    });

    expect(items).toEqual([
      { lineId: "recipe:egg|whole", label: "2 whole Eggs", name: "Eggs", category: "DairyAndEggs" },
      { lineId: "manual:m1", label: "Foil", name: "Foil", category: null, manualItemId: "m1" },
    ]);
  });
});
