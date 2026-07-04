import type { FormErrors, IngredientFormRow, RecipeFormState } from "./recipe-form-types";

export function validateRecipeForm(form: RecipeFormState): FormErrors {
  const next: FormErrors = {};

  if (form.title.trim() === "") {
    next.title = "This field is required";
  }

  if (form.instructions.trim() === "") {
    next.instructions = "This field is required";
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

  return next;
}
