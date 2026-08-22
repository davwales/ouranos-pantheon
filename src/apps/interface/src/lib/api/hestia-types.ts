export type RecipeSummary = {
  id: string;
  title: string;
  sourceUrl: string | null;
};

export type IngredientInput = {
  quantity: number;
  unit: string;
  name: string;
};

export type Ingredient = {
  quantity: number;
  unit: string;
  name: string;
};

export type Step = {
  text: string;
};

export type Recipe = {
  id: string;
  title: string;
  sourceUrl: string | null;
  steps: Step[];
  ingredients: Ingredient[];
  notes: string;
  createdAt: string;
};

export type CreateRecipeInput = {
  title: string;
  sourceUrl: string | null;
  steps: Step[];
  ingredients: IngredientInput[];
  notes: string;
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
};

export type UpdateRecipeResponse = {
  id: string;
};