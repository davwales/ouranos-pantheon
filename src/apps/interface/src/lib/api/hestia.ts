import { api } from "@/lib/api-client";
import type { PagedResponse } from "@/lib/api-client";
import type { RecipeSummary } from "./hestia-types";

export type { RecipeSummary } from "./hestia-types";

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
};
