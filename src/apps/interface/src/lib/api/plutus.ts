import { TimeFrameKey } from "@/app/plutus/constants/time_frames";
import { api, PagedResponse } from "@/lib/api-client";

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

export interface TradeBucket {
  date: string;
  maxPrice: number;
  minPrice: number;
  numTransactions: number;
  price: number;
  totalSpent: number;
  volume: number;
  openPrice: number;
  closePrice: number;
}

export interface GetSymbolTradesResponse {
  minPrice: number;
  maxPrice: number;
  averagePrice: number;
  totalSpent: number;
  volume: number;
  numTransactions: number;
  trades: TradeBucket[];
}

export interface GetMarketOverviewResponse {
  minPrice: number;
  maxPrice: number;
  averagePrice: number;
  totalSpent: number;
  volume: number;
  numTransactions: number;
  trades: TradeBucket[];
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
  latestPrice?: number | null;
  averagePrice?: number | null;
  totalValue?: number | null;
  volume?: number | null;
}

export interface SignalResponse {
  type: string;
  label: string;
  description: string;
  intents: string[];
  value: number;
  direction: string;
  strength: string;
}

export interface SignalSummary {
  aggregatedScore: number;
  bullishCount: number;
  bearishCount: number;
  neutralCount: number;
  isFlipFavourable: boolean;
  isMerchFavourable: boolean;
}

export interface GetSymbolSignalsResponse {
  signals: SignalResponse[];
  summary: SignalSummary;
}

export interface GetSignalRankingsRow {
  symbolId: string;
  symbolName: string;
  symbolSubcode?: string | null;
  dailyAveragePrice?: number | null;
  dailyVolume?: number | null;
  overallScore: number;
  buyScore?: number | null;
  sellScore?: number | null;
  flipScore?: number | null;
  merchScore?: number | null;
  signalCount: number;
  bullishCount: number;
  bearishCount: number;
}

export interface SymbolGroup {
  id: string;
  marketId: string;
  name: string;
  description?: string | null;
  symbolCount: number;
  totalVolume?: number | null;
  totalGain?: number | null;
  averageRoi?: number | null;
  averageOverallScore?: number | null;
  bullishCount: number;
  bearishCount: number;
}

export interface SymbolGroupDetail {
  id: string;
  marketId: string;
  name: string;
  description?: string | null;
  symbols: SymbolGroupSymbol[];
}

export interface SymbolGroupSymbol {
  symbolId: string;
  code: string;
  subcode?: string | null;
  name: string;
  addedAt: string;
  volume?: number | null;
  gain?: number | null;
  roi?: number | null;
  signalScore?: number | null;
}

export interface IdResponse {
  id: string;
}

export interface PageParams {
  skip?: number;
  take?: number;
  sortField?: string;
  sortDirection?: string;
  filter?: string[];
  [key: string]: string | number | boolean | string[] | undefined;
}

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

  getMarketTrades: (
    marketId: string,
    timeFrame: TimeFrameKey,
    page?: PageParams,
  ) =>
    api.get<PagedResponse<GetMarketTradesRow>>(
      `/api/plutus/markets/${marketId}/trades`,
      { timeFrame, ...page },
    ),

  getRecipeTrades: (
    marketId: string,
    timeFrame: TimeFrameKey,
    page?: PageParams,
  ) =>
    api.get<PagedResponse<GetRecipeTradesRow>>(
      `/api/plutus/markets/${marketId}/recipe-trades`,
      { timeFrame, ...page },
    ),

  getMarketOverview: (
    marketId: string,
    timeFrame: TimeFrameKey,
    numBuckets?: number,
  ) =>
    api.get<GetMarketOverviewResponse>(
      `/api/plutus/markets/${marketId}/overview`,
      { timeFrame, numBuckets },
    ),

  getSymbolTrades: (
    symbolId: string,
    timeFrame: TimeFrameKey,
    numBuckets?: number,
  ) =>
    api.get<GetSymbolTradesResponse>(`/api/plutus/symbols/${symbolId}/trades`, {
      timeFrame,
      numBuckets,
    }),

  getAllTrades: (params?: PageParams) =>
    api.get<Trade[]>("/api/plutus/trades", params),

  getAllSymbols: (params?: PageParams) =>
    api.get<PagedResponse<Symbol>>("/api/plutus/symbols", params),

  getSymbol: (symbolId: string) =>
    api.get<Symbol>(`/api/plutus/symbols/${symbolId}`),

  getDailySymbolSummary: (symbolId: string) =>
    api.get<GetDailySymbolSummaryResponse>(
      `/api/plutus/symbols/${symbolId}/summary`,
    ),

  getSymbolSignals: (symbolId: string, intent?: string) =>
    api.get<GetSymbolSignalsResponse>(
      `/api/plutus/symbols/${symbolId}/signals`,
      intent ? { intent } : undefined,
    ),

  getSignalRankings: (marketId: string, page?: PageParams) =>
    api.get<PagedResponse<GetSignalRankingsRow>>(
      `/api/plutus/markets/${marketId}/signal-rankings`,
      page,
    ),

  getAllForecasts: (params?: PageParams) =>
    api.get<PagedResponse<unknown>>("/api/plutus/forecasts", params),

  getMarketForecasts: (marketId: string, params?: PageParams) =>
    api.get<PagedResponse<GetMarketForecastRow>>(
      `/api/plutus/markets/${marketId}/forecasts`,
      params,
    ),

  getAllRecipes: (params?: PageParams) =>
    api.get<PagedResponse<Recipe>>("/api/plutus/recipes", params),

  getRecipe: (recipeId: string, timeFrame: TimeFrameKey) =>
    api.get<Recipe>(`/api/plutus/recipes/${recipeId}`, { timeFrame }),

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

  getAllSymbolGroups: (
    marketId: string,
    timeFrame: TimeFrameKey,
    page?: PageParams,
  ) =>
    api.get<PagedResponse<SymbolGroup>>("/api/plutus/symbol-groups", {
      marketId,
      timeFrame,
      ...page,
    }),

  getSymbolGroup: (symbolGroupId: string, timeFrame: TimeFrameKey) =>
    api.get<SymbolGroupDetail>(`/api/plutus/symbol-groups/${symbolGroupId}`, {
      timeFrame,
    }),

  createSymbolGroup: (input: {
    marketId: string;
    name: string;
    description?: string | null;
  }) => api.post<IdResponse>("/api/plutus/symbol-groups", input),

  updateSymbolGroup: (input: {
    symbolGroupId: string;
    name: string;
    description?: string | null;
    symbolIds: string[];
  }) =>
    api.put<IdResponse>(
      `/api/plutus/symbol-groups/${input.symbolGroupId}`,
      input,
    ),

  deleteSymbolGroup: (symbolGroupId: string) =>
    api.del<IdResponse>(`/api/plutus/symbol-groups/${symbolGroupId}`),
};
