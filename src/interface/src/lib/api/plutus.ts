import { api, PagedResponse } from "@/lib/api-client";

// ---- Domain types ----

export interface Market {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  taxes: unknown;
  isForecastingEnabled: boolean;
}

export interface Symbol {
  id: string;
  marketId: string;
  name: string;
  code: string;
  subcode?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface Trade {
  id: string;
  symbolId: string;
  marketId: string;
  symbolName: string;
  symbolCode: string;
  price: number;
  volume: number;
  timestamp: string;
}

export interface GetMarketTradesRow {
  symbolId: string;
  symbolName: string;
  symbolSubcode?: string | null;
  totalSpent: number;
  minPrice: number;
  maxPrice: number;
  totalVolume: number;
  numTransactions: number;
  limit: number;
  tax: number;
  margin: number;
  averagePrice: number;
  roi: number;
  totalGain: number;
}

export interface GetRecipeTradesRow {
  recipeId: string;
  recipeName: string;
  latestBuyPrice: number;
  latestSellPrice: number;
  latestMargin: number;
  averageBuyPrice: number;
  averageSellPrice: number;
  averageMargin: number;
}

export interface GetSymbolTradesResponse {
  minPrice: number;
  maxPrice: number;
  averagePrice: number;
  totalSpent: number;
  volume: number;
  numTransactions: number;
  trades: Array<{
    date: string;
    maxPrice: number;
    minPrice: number;
    numTransactions: number;
    price: number;
    totalSpent: number;
    volume: number;
  }>;
}

export interface GetDailySymbolSummaryResponse {
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
  volume: number;
}

export interface ForecastPoint {
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
  volume: number;
}

export interface ForecastPrediction {
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
  volume: number;
  margin: number;
  gain: number;
  averagePriceDelta: number;
  minPriceDelta: number;
  maxPriceDelta: number;
  volumeDelta: number;
  gainDelta: number;
}

export interface GetMarketForecastRow {
  id: string;
  marketId: string;
  symbolId: string;
  symbolName: string;
  symbolSubcode?: string | null;
  latest: ForecastPoint;
  dayOne: ForecastPrediction;
  dayTwo: ForecastPrediction;
  dayThree: ForecastPrediction;
  dayFour: ForecastPrediction;
  dayFive: ForecastPrediction;
  daySix: ForecastPrediction;
  daySeven: ForecastPrediction;
}

export interface Recipe {
  id: string;
  name: string;
  cost: number;
  marketId: string;
  inputs: RecipeComponent[];
  outputs: RecipeComponent[];
}

export interface RecipeComponent {
  symbolId: string;
  name: string;
  quantity: number;
}

export interface IdResponse {
  id: string;
}

// ---- Pagination helpers ----

export interface PageParams {
  skip?: number;
  take?: number;
  sortField?: string;
  sortDirection?: string;
  filter?: string[];
  [key: string]: string | number | boolean | string[] | undefined;
}

// ---- Markets ----

export const plutusApi = {
  getAllMarkets: (params?: Pick<PageParams, "filter">) =>
    api.get<Market[]>("/api/plutus/markets", params),

  getMarket: (marketId: string) =>
    api.get<Market>(`/api/plutus/markets/${marketId}`),

  createMarket: (input: {
    name: string;
    taxes: unknown;
    isForecastingEnabled?: boolean;
    description?: string;
    icon?: string;
  }) => api.post<IdResponse>("/api/plutus/markets", input),

  updateMarket: (input: { marketId: string; name: string; taxes: unknown }) =>
    api.put<IdResponse>(`/api/plutus/markets/${input.marketId}`, input),

  deleteMarket: (marketId: string) =>
    api.del<IdResponse>(`/api/plutus/markets/${marketId}`),

  // ---- Trades ----

  getMarketTrades: (marketId: string, seconds?: number, page?: PageParams) =>
    api.get<PagedResponse<GetMarketTradesRow>>(
      `/api/plutus/markets/${marketId}/trades`,
      { seconds, ...page },
    ),

  getRecipeTrades: (marketId: string, seconds?: number, page?: PageParams) =>
    api.get<PagedResponse<GetRecipeTradesRow>>(
      `/api/plutus/markets/${marketId}/recipe-trades`,
      { seconds, ...page },
    ),

  getSymbolTrades: (symbolId: string, seconds?: number, numBuckets?: number) =>
    api.get<GetSymbolTradesResponse>(`/api/plutus/symbols/${symbolId}/trades`, {
      seconds,
      numBuckets,
    }),

  getAllTrades: (params?: PageParams) =>
    api.get<Trade[]>("/api/plutus/trades", params),

  // ---- Symbols ----

  getAllSymbols: (params?: PageParams) =>
    api.get<PagedResponse<Symbol>>("/api/plutus/symbols", params),

  getSymbol: (symbolId: string) =>
    api.get<Symbol>(`/api/plutus/symbols/${symbolId}`),

  getDailySymbolSummary: (symbolId: string) =>
    api.get<GetDailySymbolSummaryResponse>(
      `/api/plutus/symbols/${symbolId}/summary`,
    ),

  // ---- Forecasts ----

  getAllForecasts: (params?: PageParams) =>
    api.get<PagedResponse<unknown>>("/api/plutus/forecasts", params),

  getMarketForecasts: (marketId: string, params?: PageParams) =>
    api.get<PagedResponse<GetMarketForecastRow>>(
      `/api/plutus/markets/${marketId}/forecasts`,
      params,
    ),

  // ---- Recipes ----

  getAllRecipes: (params?: PageParams) =>
    api.get<PagedResponse<Recipe>>("/api/plutus/recipes", params),

  getRecipe: (recipeId: string) =>
    api.get<Recipe>(`/api/plutus/recipes/${recipeId}`),

  createRecipe: (input: {
    marketId: string;
    name: string;
    cost: number;
    inputs: RecipeComponent[];
    outputs: RecipeComponent[];
  }) => api.post<IdResponse>("/api/plutus/recipes", input),

  updateRecipe: (input: {
    recipeId: string;
    marketId: string;
    name: string;
    cost: number;
    inputs: RecipeComponent[];
    outputs: RecipeComponent[];
  }) => api.put<IdResponse>(`/api/plutus/recipes/${input.recipeId}`, input),

  deleteRecipe: (recipeId: string) =>
    api.del<IdResponse>(`/api/plutus/recipes/${recipeId}`),
};
