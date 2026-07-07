export type IngredientFormRow = {
  quantity: string;
  unit: string;
  name: string;
};

export type InstructionStepFormRow = {
  text: string;
};

export type RecipeFormState = {
  title: string;
  sourceUrl: string;
  ingredients: IngredientFormRow[];
  steps: InstructionStepFormRow[];
  notes: string;
};

export type FormErrors = {
  title?: string;
  sourceUrl?: string;
  steps?: Array<{ text?: string }>;
  notes?: string;
  ingredients?: Array<{ quantity?: string; unit?: string; name?: string }>;
};