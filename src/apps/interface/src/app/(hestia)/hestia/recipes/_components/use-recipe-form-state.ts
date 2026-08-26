"use client";

import { useState } from "react";
import type { FormErrors, IngredientFormRow, RecipeFormState } from "./recipe-form-types";

export function useRecipeFormState(initialForm: RecipeFormState) {
  const [form, setForm] = useState<RecipeFormState>(initialForm);
  const [errors, setErrors] = useState<FormErrors>({});
  const [genericError, setGenericError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const clearFieldError = (field: keyof Omit<RecipeFormState, "ingredients" | "steps">) => {
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

  const clearStepError = (index: number) => {
    if (errors.steps?.[index]?.text) {
      setErrors((prev) => {
        const next = [...(prev.steps ?? [])];
        next[index] = { ...next[index], text: undefined };
        return { ...prev, steps: next };
      });
    }
  };

  const clearGenericError = () => {
    if (genericError) {
      setGenericError(null);
    }
  };

  const updateField = (
    field: keyof Omit<RecipeFormState, "ingredients" | "steps">,
    value: string,
  ) => {
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

  const updateStep = (index: number, value: string) => {
    setForm((prev) => {
      const next = [...prev.steps];
      next[index] = { ...next[index], text: value };
      return { ...prev, steps: next };
    });
    clearStepError(index);
    clearGenericError();
  };

  const addStep = () => {
    setForm((prev) => ({
      ...prev,
      steps: [...prev.steps, { id: crypto.randomUUID(), text: "" }],
    }));
  };

  const removeStep = (index: number) => {
    setForm((prev) => {
      if (prev.steps.length <= 1) {
        return prev;
      }
      const next = [...prev.steps];
      next.splice(index, 1);
      return { ...prev, steps: next };
    });

    setErrors((prev) => {
      if (!prev.steps) {
        return prev;
      }
      const next = prev.steps.filter((_, i) => i !== index);
      return { ...prev, steps: next.length > 0 ? next : undefined };
    });
  };

  const moveStepUp = (index: number) => {
    if (index <= 0) {
      return;
    }
    setForm((prev) => {
      const next = [...prev.steps];
      [next[index - 1], next[index]] = [next[index], next[index - 1]];
      return { ...prev, steps: next };
    });
    setErrors((prev) => ({ ...prev, steps: undefined }));
    clearGenericError();
  };

  const moveStepDown = (index: number) => {
    setForm((prev) => {
      if (index >= prev.steps.length - 1) {
        return prev;
      }
      const next = [...prev.steps];
      [next[index], next[index + 1]] = [next[index + 1], next[index]];
      return { ...prev, steps: next };
    });
    setErrors((prev) => ({ ...prev, steps: undefined }));
    clearGenericError();
  };

  return {
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
  };
}
