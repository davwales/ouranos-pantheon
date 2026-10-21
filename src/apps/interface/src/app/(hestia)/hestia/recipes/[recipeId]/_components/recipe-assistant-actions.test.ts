import {
  EDITING_ERROR,
  RECIPE_CHANGED_ERROR,
  recipeActionHandlers,
} from "@/app/(hestia)/hestia/recipes/[recipeId]/_components/recipe-assistant-actions";
import { type Recipe } from "@/lib/api/hestia-types";
import { describe, expect, it, vi } from "vitest";

const FLOUR = { quantity: 1.5, unit: "cups", name: "flour", isOptional: false };
const PARSLEY = { quantity: 2, unit: "tbsp", name: "parsley", isOptional: false };

const RECIPE: Recipe = {
  id: "recipe-1",
  title: "Cake",
  sourceUrl: null,
  steps: [{ text: "Mix." }, { text: "Bake for 30 minutes." }],
  ingredients: [FLOUR, PARSLEY],
  notes: "Best warm.",
  createdAt: "2026-10-01T00:00:00Z",
  importStatus: "None",
  importFailureReason: null,
  details: null,
};

function setup(isEditing = false) {
  const save = vi.fn().mockResolvedValue(undefined);
  const handlers = recipeActionHandlers({
    isEditing,
    load: () => Promise.resolve(RECIPE),
    save,
  });
  return { handlers, save };
}

describe("recipeActionHandlers", () => {
  it("WhenNoteIsApplied_ShouldAppendItToTheNotes", async () => {
    // Arrange
    const { handlers, save } = setup();

    // Act
    await handlers.add_note.apply({ text: "Freezes well." });

    // Assert
    expect(save).toHaveBeenCalledWith(
      expect.objectContaining({ notes: "Best warm.\nFreezes well." }),
    );
  });

  it("WhenIngredientIsMarkedOptional_ShouldReplaceTheMatchingIngredient", async () => {
    // Arrange
    const { handlers, save } = setup();

    // Act
    await handlers.update_ingredient.apply({
      original: PARSLEY,
      updated: { ...PARSLEY, isOptional: true },
    });

    // Assert
    expect(save).toHaveBeenCalledWith(
      expect.objectContaining({
        ingredients: [FLOUR, { ...PARSLEY, isOptional: true }],
      }),
    );
  });

  it("WhenIngredientIsAddedOrRemoved_ShouldChangeTheList", async () => {
    // Arrange
    const { handlers, save } = setup();
    const eggs = { quantity: 2, unit: "whole", name: "eggs", isOptional: false };

    // Act
    await handlers.add_ingredient.apply({ ingredient: eggs });
    await handlers.remove_ingredient.apply({ original: FLOUR });

    // Assert
    expect(save.mock.calls[0][0].ingredients).toEqual([FLOUR, PARSLEY, eggs]);
    expect(save.mock.calls[1][0].ingredients).toEqual([PARSLEY]);
  });

  it("WhenStepIsReworded_ShouldReplaceTheMatchingStep", async () => {
    // Arrange
    const { handlers, save } = setup();

    // Act
    await handlers.update_step.apply({
      original: "Bake for 30 minutes.",
      text: "Bake for 35 minutes.",
    });

    // Assert
    expect(save.mock.calls[0][0].steps).toEqual([
      { text: "Mix." },
      { text: "Bake for 35 minutes." },
    ]);
  });

  it("WhenTheTargetNoLongerExists_ShouldRejectWithoutSaving", async () => {
    // Arrange
    const { handlers, save } = setup();

    // Act & Assert
    await expect(
      handlers.remove_ingredient.apply({ original: { ...FLOUR, quantity: 3 } }),
    ).rejects.toThrow(RECIPE_CHANGED_ERROR);
    expect(save).not.toHaveBeenCalled();
  });

  it("WhenTheRecipeIsBeingEdited_ShouldRejectWithoutSaving", async () => {
    // Arrange
    const { handlers, save } = setup(true);

    // Act & Assert
    await expect(handlers.add_note.apply({ text: "Hi" })).rejects.toThrow(EDITING_ERROR);
    expect(save).not.toHaveBeenCalled();
  });

  it("ShouldDescribeProposalsInPlainWords", () => {
    // Arrange
    const { handlers } = setup();

    // Act & Assert
    expect(
      handlers.update_ingredient.describe({
        original: PARSLEY,
        updated: { ...PARSLEY, isOptional: true },
      }),
    ).toBe("Change 2 tbsp parsley to 2 tbsp parsley (optional)");
  });
});
