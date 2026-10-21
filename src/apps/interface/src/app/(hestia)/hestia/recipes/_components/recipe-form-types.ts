export type IngredientFormRow = {
  quantity: string;
  unit: string;
  name: string;
  isOptional: boolean;
};

export type IngredientTextField = Exclude<keyof IngredientFormRow, "isOptional">;

export type InstructionStepFormRow = {
  id: string;
  text: string;
};

export type RecipeFormState = {
  title: string;
  sourceUrl: string;
  ingredients: IngredientFormRow[];
  steps: InstructionStepFormRow[];
  notes: string;
  yield: string;
  prepMinutes: string;
  cookMinutes: string;
  totalMinutes: string;
  imageUrl: string;
};

export type FormErrors = {
  title?: string;
  sourceUrl?: string;
  steps?: Array<{ text?: string }>;
  notes?: string;
  yield?: string;
  prepMinutes?: string;
  cookMinutes?: string;
  totalMinutes?: string;
  imageUrl?: string;
  ingredients?: Array<{ quantity?: string; unit?: string; name?: string }>;
};