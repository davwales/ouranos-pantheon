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

export type Recipe = {
  id: string;
  title: string;
  sourceUrl: string | null;
  instructions: string;
  ingredients: Ingredient[];
  notes: string;
  createdAt: string;
};

export type CreateRecipeInput = {
  title: string;
  sourceUrl: string | null;
  instructions: string;
  ingredients: IngredientInput[];
  notes: string;
};

export type CreateRecipeResponse = {
  id: string;
};