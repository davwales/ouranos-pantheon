"use client";

import { hestiaApi } from "@/lib/api/hestia";
import { ApiError } from "@/lib/api-client";
import type { Recipe } from "@/lib/api/hestia-types";
import type { RecipeFormState } from "./recipe-form-types";
import { useRecipeFormState } from "./use-recipe-form-state";
import { validateRecipeForm } from "./validate-recipe-form";

function toFormState(data: Recipe): RecipeFormState {
  return {
    title: data.title,
    sourceUrl: data.sourceUrl ?? "",
    ingredients: data.ingredients.map((ingredient) => ({
      quantity: String(ingredient.quantity),
      unit: ingredient.unit,
      name: ingredient.name,
    })),
    steps: data.steps.map((step) => ({ id: crypto.randomUUID(), text: step.text })),
    notes: data.notes,
  };
}

export function useRecipeEditForm({
  data,
  onSaved,
}: {
  data: Recipe;
  onSaved: () => void;
}) {
  const {
    form,
    errors,
    genericError,
    isSubmitting,
    setErrors,
    setGenericError,
    setIsSubmitting,
    updateField,
    updateIngredient,
    addIngredient,
    removeIngredient,
    updateStep,
    addStep,
    removeStep,
    moveStepUp,
    moveStepDown,
  } = useRecipeFormState(toFormState(data));

  const submit = async () => {
    setGenericError(null);

    const validationErrors = validateRecipeForm(form);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});
    setIsSubmitting(true);

    try {
      const ingredients = form.ingredients
        .filter(
          (r) =>
            r.quantity.trim() !== "" || r.unit.trim() !== "" || r.name.trim() !== "",
        )
        .map((r) => ({
          quantity: parseFloat(r.quantity) || 0,
          unit: r.unit.trim(),
          name: r.name.trim(),
        }));

      const steps = form.steps
        .filter((s) => s.text.trim() !== "")
        .map((s) => ({ text: s.text.trim() }));

      await hestiaApi.updateRecipe(data.id, {
        title: form.title.trim(),
        sourceUrl: form.sourceUrl.trim() || null,
        steps,
        ingredients,
        notes: form.notes.trim(),
      });
      onSaved();
    } catch (error) {
      if (error instanceof ApiError) {
        setGenericError(error.message);
      } else if (error instanceof Error) {
        setGenericError(error.message);
      } else {
        setGenericError("An unexpected error occurred. Please try again.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return {
    form,
    errors,
    genericError,
    isSubmitting,
    updateField,
    updateIngredient,
    addIngredient,
    removeIngredient,
    updateStep,
    addStep,
    removeStep,
    moveStepUp,
    moveStepDown,
    submit,
  };
}
