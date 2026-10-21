"use client";

import { generateID } from "@/lib/utils";
import { hestiaApi } from "@/lib/api/hestia";
import { ApiError } from "@/lib/api-client";
import { useRouter } from "next/navigation";
import type { RecipeFormState } from "./recipe-form-types";
import { useRecipeFormState } from "./use-recipe-form-state";
import { toRecipeDetailsInput } from "./to-recipe-details-input";
import { validateRecipeForm } from "./validate-recipe-form";

const initialState: RecipeFormState = {
  title: "",
  sourceUrl: "",
  ingredients: [{ quantity: "", unit: "", name: "", isOptional: false }],
  steps: [{ id: generateID(), text: "" }],
  notes: "",
  yield: "",
  prepMinutes: "",
  cookMinutes: "",
  totalMinutes: "",
  imageUrl: "",
};

export function useRecipeForm() {
  const router = useRouter();
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
    setIngredientOptional,
    addIngredient,
    removeIngredient,
    updateStep,
    addStep,
    removeStep,
    moveStepUp,
    moveStepDown,
  } = useRecipeFormState(initialState);

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
          isOptional: r.isOptional,
        }));

      const steps = form.steps
        .filter((s) => s.text.trim() !== "")
        .map((s) => ({ text: s.text.trim() }));

      await hestiaApi.createRecipe({
        title: form.title.trim(),
        sourceUrl: form.sourceUrl.trim() || null,
        steps,
        ingredients,
        notes: form.notes.trim(),
        details: toRecipeDetailsInput(form),
      });
      router.push("/hestia/recipes");
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
    setIngredientOptional,
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
