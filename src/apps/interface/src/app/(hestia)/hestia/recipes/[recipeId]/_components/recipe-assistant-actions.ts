import { type AssistantActionHandlers } from "@/lib/api/assistant";
import {
  type AddIngredientAction,
  type AddRecipeNoteAction,
  type Ingredient,
  type ProposedIngredient,
  type Recipe,
  type RemoveIngredientAction,
  type UpdateIngredientAction,
  type UpdateRecipeInput,
  type UpdateStepAction,
} from "@/lib/api/hestia-types";

export const RECIPE_CHANGED_ERROR = "The recipe has changed since this was proposed; ask again.";
export const EDITING_ERROR = "Save or cancel your edits before applying this change.";

type RecipeEdit = (recipe: Recipe) => Recipe;

function formatIngredient(ingredient: ProposedIngredient): string {
  const quantity = ingredient.quantity > 0 ? `${ingredient.quantity} ` : "";
  const optional = ingredient.isOptional ? " (optional)" : "";
  return `${quantity}${ingredient.unit} ${ingredient.name}${optional}`;
}

function sameIngredient(a: Ingredient, b: ProposedIngredient): boolean {
  return (
    a.quantity === b.quantity &&
    a.unit === b.unit &&
    a.name === b.name &&
    a.isOptional === b.isOptional
  );
}

// Proposals name the ingredient or step they change rather than its position, so applying one
// proposal never shifts the target of another.
function indexOfIngredient(recipe: Recipe, original: ProposedIngredient): number {
  const index = recipe.ingredients.findIndex((i) => sameIngredient(i, original));
  if (index < 0) {
    throw new Error(RECIPE_CHANGED_ERROR);
  }
  return index;
}

const EDITS = {
  add_note: (action: AddRecipeNoteAction): RecipeEdit => (recipe) => ({
    ...recipe,
    notes: recipe.notes ? `${recipe.notes}\n${action.text}` : action.text,
  }),
  add_ingredient: (action: AddIngredientAction): RecipeEdit => (recipe) => ({
    ...recipe,
    ingredients: [...recipe.ingredients, action.ingredient],
  }),
  update_ingredient: (action: UpdateIngredientAction): RecipeEdit => (recipe) => {
    const index = indexOfIngredient(recipe, action.original);
    return {
      ...recipe,
      ingredients: recipe.ingredients.map((i, n) => (n === index ? action.updated : i)),
    };
  },
  remove_ingredient: (action: RemoveIngredientAction): RecipeEdit => (recipe) => {
    const index = indexOfIngredient(recipe, action.original);
    return { ...recipe, ingredients: recipe.ingredients.filter((_, n) => n !== index) };
  },
  update_step: (action: UpdateStepAction): RecipeEdit => (recipe) => {
    const index = recipe.steps.findIndex((s) => s.text === action.original);
    if (index < 0) {
      throw new Error(RECIPE_CHANGED_ERROR);
    }
    return {
      ...recipe,
      steps: recipe.steps.map((s, n) => (n === index ? { text: action.text } : s)),
    };
  },
};

const DESCRIPTIONS = {
  add_note: (action: AddRecipeNoteAction) => `Add note: “${action.text}”`,
  add_ingredient: (action: AddIngredientAction) =>
    `Add ingredient: ${formatIngredient(action.ingredient)}`,
  update_ingredient: (action: UpdateIngredientAction) =>
    `Change ${formatIngredient(action.original)} to ${formatIngredient(action.updated)}`,
  remove_ingredient: (action: RemoveIngredientAction) =>
    `Remove ingredient: ${formatIngredient(action.original)}`,
  update_step: (action: UpdateStepAction) => `Reword step: “${action.text}”`,
};

export function toUpdateRecipeInput(recipe: Recipe): UpdateRecipeInput {
  return {
    title: recipe.title,
    sourceUrl: recipe.sourceUrl,
    steps: recipe.steps,
    ingredients: recipe.ingredients,
    notes: recipe.notes,
  };
}

export function recipeActionHandlers({
  isEditing,
  load,
  save,
}: {
  isEditing: boolean;
  load: () => Promise<Recipe>;
  save: (input: UpdateRecipeInput) => Promise<void>;
}): AssistantActionHandlers {
  const handler = <TAction>(
    edit: (action: TAction) => RecipeEdit,
    describe: (action: TAction) => string,
  ) => ({
    describe,
    apply: async (action: TAction) => {
      if (isEditing) {
        throw new Error(EDITING_ERROR);
      }
      const recipe = await load();
      await save(toUpdateRecipeInput(edit(action)(recipe)));
    },
  });

  return {
    add_note: handler(EDITS.add_note, DESCRIPTIONS.add_note),
    add_ingredient: handler(EDITS.add_ingredient, DESCRIPTIONS.add_ingredient),
    update_ingredient: handler(EDITS.update_ingredient, DESCRIPTIONS.update_ingredient),
    remove_ingredient: handler(EDITS.remove_ingredient, DESCRIPTIONS.remove_ingredient),
    update_step: handler(EDITS.update_step, DESCRIPTIONS.update_step),
  };
}
