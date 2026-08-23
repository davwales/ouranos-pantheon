const RECIPE_HISTORY_EVENT_LABELS: Record<string, string> = {
  recipe_created: "Recipe Created",
  recipe_title_changed: "Title Updated",
  recipe_source_url_changed: "Source URL Updated",
  recipe_steps_changed: "Instructions Modified",
  recipe_ingredients_changed: "Ingredients Updated",
  recipe_notes_changed: "Notes Updated",
  recipe_reverted: "Recipe Reverted",
};

export function recipeHistoryEventLabel(eventType: string): string {
  return RECIPE_HISTORY_EVENT_LABELS[eventType] ?? "Unknown change";
}
