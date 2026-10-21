import type { RecipeDetails } from "@/lib/api/hestia-types";
import type { RecipeFormState } from "./recipe-form-types";

function toMinutes(value: string): number | null {
  const trimmed = value.trim();
  return trimmed === "" ? null : Number(trimmed);
}

export function toRecipeDetailsInput(form: RecipeFormState): RecipeDetails {
  return {
    yield: form.yield.trim() || null,
    prepMinutes: toMinutes(form.prepMinutes),
    cookMinutes: toMinutes(form.cookMinutes),
    totalMinutes: toMinutes(form.totalMinutes),
    imageUrl: form.imageUrl.trim() || null,
  };
}
