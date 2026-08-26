const RECIPE_HISTORY_EVENT_LABELS: Record<string, string> = {
  recipe_source_url_changed: "Source URL Updated",
  recipe_steps_changed: "Instructions Modified",
};

export function recipeHistoryEventLabel(eventType: string): string {
  const override = RECIPE_HISTORY_EVENT_LABELS[eventType];

  if (override !== undefined) {
    return override;
  }

  const words = eventType
    .replace(/^recipe_/, "")
    .split("_")
    .filter((word) => word.length > 0);

  if (words.length === 0) {
    return eventType;
  }

  return words
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
}
