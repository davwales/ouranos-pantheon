import { describe, expect, it } from "vitest";
import type { RecipeFormState } from "./recipe-form-types";
import { validateRecipeForm } from "./validate-recipe-form";

function validForm(overrides: Partial<RecipeFormState> = {}): RecipeFormState {
  return {
    title: "Cake",
    sourceUrl: "",
    ingredients: [{ quantity: "2", unit: "cup", name: "flour", isOptional: false }],
    steps: [{ id: "1", text: "Bake." }],
    notes: "",
    yield: "",
    prepMinutes: "",
    cookMinutes: "",
    totalMinutes: "",
    imageUrl: "",
    ...overrides,
  };
}

describe("validateRecipeForm", () => {
  it("accepts a form with blank details", () => {
    expect(validateRecipeForm(validForm())).toEqual({});
  });

  it("accepts valid details", () => {
    const form = validForm({
      yield: "4 servings",
      prepMinutes: "15",
      cookMinutes: "0",
      totalMinutes: "10080",
      imageUrl: "https://example.com/cake.jpg",
    });

    expect(validateRecipeForm(form)).toEqual({});
  });

  it.each(["-5", "1.5", "abc", "10081"])("rejects %s as minutes", (value) => {
    const errors = validateRecipeForm(
      validForm({ prepMinutes: value, cookMinutes: value, totalMinutes: value }),
    );

    expect(errors.prepMinutes).toBeDefined();
    expect(errors.cookMinutes).toBeDefined();
    expect(errors.totalMinutes).toBeDefined();
  });

  it.each(["not a url", "ftp://example.com/a.jpg", "javascript:alert(1)"])(
    "rejects %s as an image URL",
    (value) => {
      expect(validateRecipeForm(validForm({ imageUrl: value })).imageUrl).toBeDefined();
    },
  );

  it("rejects a yield longer than 100 characters", () => {
    expect(validateRecipeForm(validForm({ yield: "x".repeat(101) })).yield).toBeDefined();
  });
});
