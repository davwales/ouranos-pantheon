"use client";

import { hestiaApi } from "@/lib/api/hestia";
import { ApiError } from "@/lib/api-client";
import { useRouter } from "next/navigation";
import { useState } from "react";
import type { FormErrors, IngredientFormRow, RecipeFormState } from "./recipe-form-types";
import { validateRecipeForm } from "./validate-recipe-form";

const initialState: RecipeFormState = {
  title: "",
  sourceUrl: "",
  ingredients: [{ quantity: "", unit: "", name: "" }],
  instructions: "",
  notes: "",
};

export function useRecipeForm() {
  const router = useRouter();
  const [form, setForm] = useState<RecipeFormState>(initialState);
  const [errors, setErrors] = useState<FormErrors>({});
  const [genericError, setGenericError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const clearFieldError = (field: keyof Omit<RecipeFormState, "ingredients">) => {
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const clearIngredientError = (index: number, field: keyof IngredientFormRow) => {
    if (errors.ingredients?.[index]?.[field]) {
      setErrors((prev) => {
        const next = [...(prev.ingredients ?? [])];
        next[index] = { ...next[index], [field]: undefined };
        return { ...prev, ingredients: next };
      });
    }
  };

  const clearGenericError = () => {
    if (genericError) {
      setGenericError(null);
    }
  };

  const updateField = (field: keyof Omit<RecipeFormState, "ingredients">, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    clearFieldError(field);
    clearGenericError();
  };

  const updateIngredient = (index: number, field: keyof IngredientFormRow, value: string) => {
    setForm((prev) => {
      const next = [...prev.ingredients];
      next[index] = { ...next[index], [field]: value };
      return { ...prev, ingredients: next };
    });
    clearIngredientError(index, field);
    clearGenericError();
  };

  const addIngredient = () => {
    setForm((prev) => ({
      ...prev,
      ingredients: [...prev.ingredients, { quantity: "", unit: "", name: "" }],
    }));
  };

  const removeIngredient = (index: number) => {
    setForm((prev) => {
      if (prev.ingredients.length <= 1) {
        return prev;
      }
      const next = [...prev.ingredients];
      next.splice(index, 1);
      return { ...prev, ingredients: next };
    });

    setErrors((prev) => {
      if (!prev.ingredients) {
        return prev;
      }
      const next = prev.ingredients.filter((_, i) => i !== index);
      return { ...prev, ingredients: next.length > 0 ? next : undefined };
    });
  };

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

      await hestiaApi.createRecipe({
        title: form.title.trim(),
        sourceUrl: form.sourceUrl.trim() || null,
        instructions: form.instructions.trim(),
        ingredients,
        notes: form.notes.trim(),
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
    addIngredient,
    removeIngredient,
    submit,
  };
}
