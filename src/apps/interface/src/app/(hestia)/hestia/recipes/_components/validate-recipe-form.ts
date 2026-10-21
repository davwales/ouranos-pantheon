import type { FormErrors, RecipeFormState } from "./recipe-form-types";

const MAX_YIELD_LENGTH = 100;
const MAX_MINUTES = 10_080;
const MINUTE_FIELDS = ["prepMinutes", "cookMinutes", "totalMinutes"] as const;

function isHttpUrl(value: string): boolean {
  try {
    const url = new URL(value);
    return url.protocol === "http:" || url.protocol === "https:";
  } catch {
    return false;
  }
}

export function validateRecipeForm(form: RecipeFormState): FormErrors {
  const next: FormErrors = {};

  if (form.title.trim() === "") {
    next.title = "This field is required";
  }

  const stepErrors: FormErrors["steps"] = [];
  let hasAtLeastOneStepText = false;

  for (const row of form.steps) {
    const rowErrors: { text?: string } = {};
    const text = row.text.trim();

    if (text !== "") {
      hasAtLeastOneStepText = true;

      if (text.length > 2000) {
        rowErrors.text = "Step must be 2000 characters or less";
      }
    }

    stepErrors.push(rowErrors);
  }

  if (!hasAtLeastOneStepText) {
    stepErrors[0] = {
      ...stepErrors[0],
      text: "At least one step is required",
    };
  }

  if (stepErrors.some((e) => Object.keys(e).length > 0)) {
    next.steps = stepErrors;
  }

  const ingredientErrors: FormErrors["ingredients"] = [];
  let hasAtLeastOneName = false;

  for (const row of form.ingredients) {
    const rowErrors: { quantity?: string; unit?: string; name?: string } = {};
    const name = row.name.trim();
    const unit = row.unit.trim();
    const quantityValue = row.quantity.trim();
    const quantity = quantityValue === "" ? undefined : parseFloat(quantityValue);

    if (name !== "") {
      hasAtLeastOneName = true;

      if (quantity === undefined || Number.isNaN(quantity) || quantity < 0) {
        rowErrors.quantity = "Enter a valid quantity";
      }

      if (unit === "") {
        rowErrors.unit = "Unit is required";
      } else if (unit.length > 50) {
        rowErrors.unit = "Unit must be 50 characters or less";
      }

      if (name.length > 200) {
        rowErrors.name = "Name must be 200 characters or less";
      }
    } else if (unit !== "" || quantity !== undefined) {
      rowErrors.name = "Name is required";
    }

    ingredientErrors.push(rowErrors);
  }

  if (!hasAtLeastOneName) {
    ingredientErrors[0] = {
      ...ingredientErrors[0],
      name: "At least one ingredient is required",
    };
  }

  if (ingredientErrors.some((e) => Object.keys(e).length > 0)) {
    next.ingredients = ingredientErrors;
  }

  if (form.yield.trim().length > MAX_YIELD_LENGTH) {
    next.yield = `Yield must be ${MAX_YIELD_LENGTH} characters or less`;
  }

  for (const field of MINUTE_FIELDS) {
    const value = form[field].trim();
    if (value !== "" && (!/^\d+$/.test(value) || Number(value) > MAX_MINUTES)) {
      next[field] = `Enter whole minutes up to ${MAX_MINUTES}`;
    }
  }

  const imageUrl = form.imageUrl.trim();
  if (imageUrl !== "" && (!isHttpUrl(imageUrl) || imageUrl.length > 2000)) {
    next.imageUrl = "Enter a valid http(s) URL";
  }

  return next;
}