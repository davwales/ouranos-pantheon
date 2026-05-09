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

export interface MarketOverviewBucket {
  date: string;
  numTransactions: number;
  price: number;
  totalSpent: number;
  volume: number;
}

export interface GetMarketOverviewResponse {
  averagePrice: number;
  totalSpent: number;
  volume: number;
  numTransactions: number;
  trades: MarketOverviewBucket[];
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

export type StrategyType =
  | "SignalWeighted"
  | "ForecastMomentum"
  | "MeanReversion"
  | "RecipeArbitrage"
  | "Composite";

export type BacktestStatus =
  | "Pending"
  | "Running"
  | "Completed"
  | "Failed"
  | "Cancelled";

export type BacktestKind = "Backtest" | "Optimization";

export interface SignalWeight {
  type: string;
  weight: number;
}

export interface CompositeComponent {
  strategyId: string;
  type: StrategyType;
  weight: number;
}

export interface StrategyConfiguration {
  signalWeights?: SignalWeight[] | null;
  buyThreshold?: number | null;
  sellThreshold?: number | null;
  forecastMovementThreshold?: number | null;
  forecastHorizonDays?: number | null;
  deviationMultiplier?: number | null;
  meanTimeFrameValue?: number | null;
  minMarginPercent?: number | null;
  components?: CompositeComponent[] | null;
  maxPositions?: number | null;
  maxPositionPercent?: number | null;
  holdPeriodDays?: number | null;
}

export interface Strategy {
  id: string;
  marketId: string;
  name: string;
  description?: string | null;
  type: StrategyType;
  isActive: boolean;
  createdAt: string;
  backtestCount: number;
  lastBacktestReturn?: number | null;
  lastBacktestWinRate?: number | null;
}

export interface StrategyDetail {
  id: string;
  marketId: string;
  name: string;
  description?: string | null;
  type: StrategyType;
  configuration: StrategyConfiguration;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface BacktestSummary {
  id: string;
  marketId: string;
  startDate: string;
  endDate: string;
  budget: number;
  kind: BacktestKind;
  status: BacktestStatus;
  totalReturnPercent?: number | null;
  winRate?: number | null;
  sharpeRatio?: number | null;
  totalTrades?: number | null;
  createdAt: string;
}

export interface BacktestPosition {
  symbolId: string;
  symbolName: string;
  entryPrice: number;
  exitPrice: number;
  volume: number;
  profitLoss: number;
  returnPercent: number;
  entryTime: string;
  exitTime: string;
}

export interface BacktestResults {
  totalReturn: number;
  totalReturnPercent: number;
  maxDrawdown: number;
  maxDrawdownPercent: number;
  winRate: number;
  totalTrades: number;
  winningTrades: number;
  losingTrades: number;
  sharpeRatio: number;
  sortinoRatio: number | null;
  calmarRatio: number | null;
  cagr: number | null;
  profitFactor: number | null;
  expectancy: number | null;
  averageTradeReturn: number;
  bestTrade: number;
  worstTrade: number;
  finalBalance: number;
  positions: BacktestPosition[];
  optimizedConfiguration?: StrategyConfiguration | null;
}

export interface BacktestDetail {
  id: string;
  strategyId: string;
  marketId: string;
  startDate: string;
  endDate: string;
  budget: number;
  kind: BacktestKind;
  status: BacktestStatus;
  progressPercent: number;
  progressMessage: string | null;
  results?: BacktestResults | null;
  errorMessage?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface RunBacktestResponse {
  backtestId: string;
}

export interface CancelBacktestResponse {
  backtestId: string;
  status: BacktestStatus;
}

export interface RestartBacktestResponse {
  backtestId: string;
  status: BacktestStatus;
}

export interface StrategyRecommendation {
  symbolId: string;
  symbolName: string;
  symbolSubcode?: string | null;
  score: number;
  suggestedAllocation: number;
  currentPrice: number;
  suggestedVolume: number;
  rationale: string;
}

export interface GetRecommendationsResponse {
  recommendations: StrategyRecommendation[];
}

export type PositionSide = "Buy" | "Sell";

export type PositionStatus =
  | "Pending"
  | "DidNotBuy"
  | "Bought"
  | "DidNotSell"
  | "Sold";

export interface Position {
  id: string;
  side: PositionSide;
  status: PositionStatus;
  marketId: string;
  symbolId: string;
  symbolName: string;
  cost: number;
  quantity: number;
  linkedBuyPositionId: string | null;
  strategyId: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ClosePositionResponse {
  positionId: string;
  status: PositionStatus;
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

  getAllStrategies: (marketId: string, page?: PageParams) =>
    api.get<PagedResponse<Strategy>>("/api/plutus/strategies", {
      marketId,
      ...page,
    }),

  getStrategy: (strategyId: string) =>
    api.get<StrategyDetail>(`/api/plutus/strategies/${strategyId}`),

  createStrategy: (input: {
    marketId: string;
    name: string;
    description?: string | null;
    type: StrategyType;
    configuration: StrategyConfiguration;
  }) => api.post<IdResponse>("/api/plutus/strategies", input),

  updateStrategy: (
    strategyId: string,
    input: {
      name: string;
      description?: string | null;
      configuration: StrategyConfiguration;
    },
  ) => api.put<IdResponse>(`/api/plutus/strategies/${strategyId}`, input),

  deleteStrategy: (strategyId: string) =>
    api.del<IdResponse>(`/api/plutus/strategies/${strategyId}`),

  setStrategyActive: (strategyId: string, isActive: boolean) =>
    api.patch<IdResponse>(`/api/plutus/strategies/${strategyId}/active`, {
      isActive,
    }),

  runBacktest: (
    strategyId: string,
    input: {
      marketId: string;
      startDate: string;
      endDate: string;
      budget: number;
      volumeParticipationRate?: number;
      slippageMultiplier?: number;
    },
  ) =>
    api.post<RunBacktestResponse>(
      `/api/plutus/strategies/${strategyId}/backtest`,
      input,
    ),

  getAllBacktests: (strategyId: string, page?: PageParams) =>
    api.get<PagedResponse<BacktestSummary>>(
      `/api/plutus/strategies/${strategyId}/backtests`,
      {
        ...page,
      },
    ),

  getBacktest: (backtestId: string) =>
    api.get<BacktestDetail>(`/api/plutus/backtests/${backtestId}`),

  cancelBacktest: (backtestId: string, reason?: string) =>
    api.post<CancelBacktestResponse>(
      `/api/plutus/backtests/${backtestId}/cancel`,
      reason ? { reason } : {},
    ),

  restartBacktest: (backtestId: string) =>
    api.post<RestartBacktestResponse>(
      `/api/plutus/backtests/${backtestId}/restart`,
      {},
    ),

  getRecommendations: (
    strategyId: string,
    input: {
      marketId: string;
      budget: number;
    },
  ) =>
    api.post<GetRecommendationsResponse>(
      `/api/plutus/strategies/${strategyId}/recommendations`,
      input,
    ),

  optimizeStrategy: (
    strategyId: string,
    input: {
      marketId: string;
      startDate: string;
      endDate: string;
      budget: number;
      generations?: number;
      populationSize?: number;
      sharpeRatioWeight?: number;
      totalReturnWeight?: number;
      maxDrawdownWeight?: number;
      volumeParticipationRate?: number;
      slippageMultiplier?: number;
    },
  ) =>
    api.post<RunBacktestResponse>(
      `/api/plutus/strategies/${strategyId}/optimize`,
      input,
    ),

  getAllPositions: (marketId: string, page?: PageParams) =>
    api.get<PagedResponse<Position>>("/api/plutus/positions", {
      marketId,
      ...page,
    }),

  getPosition: (positionId: string) =>
    api.get<Position>(`/api/plutus/positions/${positionId}`),

  createPosition: (input: {
    side: PositionSide;
    marketId: string;
    symbolId: string;
    cost: number;
    quantity: number;
    strategyId?: string | null;
    notes?: string | null;
  }) => api.post<IdResponse>("/api/plutus/positions", input),

  updatePosition: (
    positionId: string,
    input: {
      cost: number;
      quantity: number;
      notes?: string | null;
    },
  ) => api.put<IdResponse>(`/api/plutus/positions/${positionId}`, input),

  closePosition: (positionId: string, closeStatus: PositionStatus) =>
    api.post<ClosePositionResponse>(
      `/api/plutus/positions/${positionId}/close`,
      { closeStatus },
    ),

  linkPosition: (positionId: string, targetPositionId: string) =>
    api.post<IdResponse>(`/api/plutus/positions/${positionId}/link`, {
      targetPositionId,
    }),
};
