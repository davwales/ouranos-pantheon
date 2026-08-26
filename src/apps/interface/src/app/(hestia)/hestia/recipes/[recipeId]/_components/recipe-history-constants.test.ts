import { describe, expect, it } from "vitest";
import { recipeHistoryEventLabel } from "./recipe-history-constants";

describe("recipeHistoryEventLabel", () => {
  it("uses curated overrides when defined", () => {
    expect(recipeHistoryEventLabel("recipe_steps_changed")).toBe(
      "Instructions Modified",
    );
    expect(recipeHistoryEventLabel("recipe_source_url_changed")).toBe(
      "Source URL Updated",
    );
  });

  it.each([
    ["recipe_created", "Created"],
    ["recipe_title_changed", "Title Changed"],
    ["recipe_ingredients_changed", "Ingredients Changed"],
    ["recipe_notes_changed", "Notes Changed"],
    ["recipe_reverted", "Reverted"],
  ])("derives a label for %s", (eventType, expected) => {
    expect(recipeHistoryEventLabel(eventType)).toBe(expected);
  });

  it.each([
    ["recipe_import_started", "Import Started"],
    ["recipe_import_failed", "Import Failed"],
    ["recipe_import_succeeded", "Import Succeeded"],
  ])("derives a label for the import event %s", (eventType, expected) => {
    expect(recipeHistoryEventLabel(eventType)).toBe(expected);
  });

  it("derives a label for unknown future events", () => {
    expect(recipeHistoryEventLabel("recipe_photo_attached")).toBe(
      "Photo Attached",
    );
  });

  it("humanizes event types without the recipe_ prefix", () => {
    expect(recipeHistoryEventLabel("magic_happened")).toBe("Magic Happened");
  });

  it("returns the input when nothing can be derived", () => {
    expect(recipeHistoryEventLabel("")).toBe("");
    expect(recipeHistoryEventLabel("recipe_")).toBe("recipe_");
  });
});
