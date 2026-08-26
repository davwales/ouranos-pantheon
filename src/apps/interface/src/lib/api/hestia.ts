import { api } from "@/lib/api-client";
import type { PagedResponse } from "@/lib/api-client";
import type {
  CreateRecipeInput,
  CreateRecipeResponse,
  ImportRecipeInput,
  ImportRecipeResponse,
  Recipe,
  RecipeHistoryResponse,
  RecipeSummary,
  ReimportRecipeResponse,
  RevertRecipeInput,
  RevertRecipeResponse,
  UpdateRecipeInput,
  UpdateRecipeResponse,
} from "./hestia-types";

export type {
  CreateRecipeInput,
  CreateRecipeResponse,
  ImportRecipeInput,
  ImportRecipeResponse,
  Ingredient,
  Recipe,
  RecipeHistoryEvent,
  RecipeHistoryResponse,
  RecipeImportStatus,
  RecipeSummary,
  ReimportRecipeResponse,
  RevertRecipeInput,
  RevertRecipeResponse,
  Step,
  UpdateRecipeInput,
  UpdateRecipeResponse,
} from "./hestia-types";

export type GetAllRecipesParams = {
  skip?: number;
  take?: number;
  sortField?: string;
  sortDirection?: string;
  filter?: string[];
};

export const hestiaApi = {
  getAllRecipes: (params?: GetAllRecipesParams) =>
    api.get<PagedResponse<RecipeSummary>>("/api/hestia/recipes", params),

  getRecipe: (recipeId: string) =>
    api.get<Recipe>(`/api/hestia/recipes/${recipeId}`),

  createRecipe: (input: CreateRecipeInput) =>
    api.post<CreateRecipeResponse>("/api/hestia/recipes", input),

  importRecipe: (input: ImportRecipeInput) =>
    api.post<ImportRecipeResponse>("/api/hestia/recipes/import", input),

  reimportRecipe: (recipeId: string) =>
    api.post<ReimportRecipeResponse>(
      `/api/hestia/recipes/${recipeId}/reimport`,
    ),

  updateRecipe: (recipeId: string, input: UpdateRecipeInput) =>
    api.put<UpdateRecipeResponse>(`/api/hestia/recipes/${recipeId}`, input),

  getRecipeHistory: (recipeId: string) =>
    api.get<RecipeHistoryResponse>(`/api/hestia/recipes/${recipeId}/history`),

  revertRecipe: (recipeId: string, input: RevertRecipeInput) =>
    api.post<RevertRecipeResponse>(`/api/hestia/recipes/${recipeId}/revert`, input),
};
