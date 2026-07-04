export type IngredientFormRow = {
  quantity: string;
  unit: string;
  name: string;
};

export type RecipeFormState = {
  title: string;
  sourceUrl: string;
  ingredients: IngredientFormRow[];
  instructions: string;
  notes: string;
};

export type FormErrors = {
  title?: string;
  sourceUrl?: string;
  instructions?: string;
  notes?: string;
  ingredients?: Array<{ quantity?: string; unit?: string; name?: string }>;
};
