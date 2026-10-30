export type RecipeSummary = {
  id: string;
  title: string;
  sourceUrl: string | null;
  createdAt: string;
  ingredientCount: number;
  stepCount: number;
  importStatus: RecipeImportStatus;
};

export type IngredientInput = {
  quantity: number;
  unit: string;
  name: string;
  isOptional: boolean;
};

export type Ingredient = {
  quantity: number;
  unit: string;
  name: string;
  isOptional: boolean;
};

export type Step = {
  text: string;
};

export type RecipeDetails = {
  yield: string | null;
  prepMinutes: number | null;
  cookMinutes: number | null;
  totalMinutes: number | null;
  imageUrl: string | null;
};

export type RecipeImportStatus = "None" | "Importing" | "Imported" | "Failed";

export type Recipe = {
  id: string;
  title: string;
  sourceUrl: string | null;
  steps: Step[];
  ingredients: Ingredient[];
  notes: string;
  createdAt: string;
  importStatus: RecipeImportStatus;
  importFailureReason: string | null;
  details: RecipeDetails | null;
};

export type ImportRecipeInput = {
  url: string;
};

export type ImportRecipeResponse = {
  id: string;
};

export type ReimportRecipeResponse = {
  id: string;
};

export type CreateRecipeInput = {
  title: string;
  sourceUrl: string | null;
  steps: Step[];
  ingredients: IngredientInput[];
  notes: string;
  details?: RecipeDetails;
};

export type CreateRecipeResponse = {
  id: string;
};

export type UpdateRecipeInput = {
  title: string;
  sourceUrl: string | null;
  steps: Step[];
  ingredients: IngredientInput[];
  notes: string;
  details?: RecipeDetails;
};

export type UpdateRecipeResponse = {
  id: string;
};

export type RecipeHistoryEvent = {
  version: number;
  eventType: string;
  timestamp: string;
};

export type RecipeHistoryResponse = {
  recipeId: string;
  events: RecipeHistoryEvent[];
};

export type RevertRecipeInput = {
  targetVersion: number;
};

export type RevertRecipeResponse = {
  id: string;
};

export type ShoppingCategory =
  | "Produce"
  | "Bakery"
  | "MeatAndSeafood"
  | "DairyAndEggs"
  | "Frozen"
  | "Pantry"
  | "SpicesAndSeasonings"
  | "Beverages"
  | "Household";

export type ConsolidatedIngredient = {
  id: string;
  name: string;
  unit: string;
  quantity: number;
  category: ShoppingCategory | null;
};

export type ManualItem = {
  id: string;
  text: string;
  category: ShoppingCategory | null;
};

export type ShoppingListRecipe = {
  id: string;
  title: string;
};

export type ShoppingListResponse = {
  recipeIds: string[];
  recipes: ShoppingListRecipe[];
  consolidatedIngredients: ConsolidatedIngredient[];
  manualItems: ManualItem[];
  checkedItemIds: string[];
};

export type ToggleRecipeResponse = {
  recipeId: string;
  isInList: boolean;
};

export type AddManualItemInput = {
  text: string;
};

export type DeleteManualItemResponse = {
  id: string;
};

export type UpdateCheckedItemsInput = {
  checkedItemIds: string[];
};

export type UpdateCheckedItemsResponse = {
  checkedItemIds: string[];
};

export type ClearCheckedItemsResponse = {
  removedManualItemIds: string[];
  removedRecipeIds: string[];
};

export type SetItemCategoryInput = {
  name: string;
  category: ShoppingCategory | null;
};

export type SetItemCategoryResponse = SetItemCategoryInput;

export type ProposedIngredient = {
  quantity: number;
  unit: string;
  name: string;
  isOptional: boolean;
};

export type AddRecipeNoteAction = { text: string };

export type AddIngredientAction = { ingredient: ProposedIngredient };

export type UpdateIngredientAction = {
  original: ProposedIngredient;
  updated: ProposedIngredient;
};

export type RemoveIngredientAction = { original: ProposedIngredient };

export type UpdateStepAction = { original: string; text: string };

export type KitchenAssistantContext = {
  recipeId: string;
};
