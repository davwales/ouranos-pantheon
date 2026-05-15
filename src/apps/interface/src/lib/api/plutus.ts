import { TimeFrameKey } from "@/app/(plutus)/plutus/constants/time-frames";
import { api, PagedResponse } from "@/lib/api-client";

export type {
  Market,
  Symbol,
  Trade,
  GetMarketTradesRow,
  GetRecipeTradesRow,
  TradeBucket,
  GetSymbolTradesResponse,
  MarketOverviewBucket,
  GetMarketOverviewResponse,
  GetDailySymbolSummaryResponse,
  ForecastPoint,
  ForecastPrediction,
  GetMarketForecastRow,
  Recipe,
  RecipeComponent,
  SignalResponse,
  SignalSummary,
  GetSymbolSignalsResponse,
  GetSignalRankingsRow,
  SymbolGroup,
  SymbolGroupDetail,
  SymbolGroupSymbol,
  IdResponse,
  StrategyType,
  BacktestStatus,
  BacktestKind,
  SignalWeight,
  CompositeComponent,
  TradingConfiguration,
  SignalWeightedConfig,
  ForecastMomentumConfig,
  MeanReversionConfig,
  RecipeArbitrageConfig,
  StrategyConfigBundle,
  Strategy,
  StrategyDetail,
  BacktestSummary,
  BacktestPosition,
  BacktestResults,
  BacktestDetail,
  RunBacktestResponse,
  CancelBacktestResponse,
  RestartBacktestResponse,
  StrategyRecommendation,
  GetRecommendationsResponse,
  PositionSide,
  PositionStatus,
  Position,
  ClosePositionResponse,
  ForecastEfficacyRow,
  PageParams,
} from "./plutus-types";

import type {
  Market,
  Symbol,
  Trade,
  GetMarketTradesRow,
  GetRecipeTradesRow,
  GetMarketOverviewResponse,
  GetSymbolTradesResponse,
  GetDailySymbolSummaryResponse,
  GetSymbolSignalsResponse,
  GetSignalRankingsRow,
  GetMarketForecastRow,
  ForecastEfficacyRow,
  Recipe,
  RecipeComponent,
  SymbolGroup,
  SymbolGroupDetail,
  IdResponse,
  Strategy,
  StrategyDetail,
  BacktestSummary,
  BacktestDetail,
  RunBacktestResponse,
  CancelBacktestResponse,
  RestartBacktestResponse,
  GetRecommendationsResponse,
  Position,
  ClosePositionResponse,
  PageParams,
} from "./plutus-types";
import type {
  StrategyType,
  TradingConfiguration,
  SignalWeightedConfig,
  ForecastMomentumConfig,
  MeanReversionConfig,
  RecipeArbitrageConfig,
  CompositeComponent,
  BacktestKind,
  BacktestStatus,
  PositionSide,
  PositionStatus,
} from "./plutus-types";

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

  getForecastEfficacy: (
    params?: {
      symbolId?: string;
      marketId?: string;
      modelName?: string;
      horizonDays?: number;
      since?: string;
      until?: string;
      skip?: number;
      take?: number;
    },
  ) =>
    api.get<PagedResponse<ForecastEfficacyRow>>(
      "/api/plutus/forecasts/efficacy",
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
    tradingConfiguration: TradingConfiguration;
    signalWeightedConfig?: SignalWeightedConfig | null;
    forecastMomentumConfig?: ForecastMomentumConfig | null;
    meanReversionConfig?: MeanReversionConfig | null;
    recipeArbitrageConfig?: RecipeArbitrageConfig | null;
    components?: CompositeComponent[] | null;
  }) => {
    const { tradingConfiguration, ...rest } = input;
    return api.post<IdResponse>("/api/plutus/strategies", {
      ...rest,
      configuration: tradingConfiguration,
    });
  },

  updateStrategy: (
    strategyId: string,
    input: {
      name: string;
      description?: string | null;
      tradingConfiguration: TradingConfiguration;
      signalWeightedConfig?: SignalWeightedConfig | null;
      forecastMomentumConfig?: ForecastMomentumConfig | null;
      meanReversionConfig?: MeanReversionConfig | null;
      recipeArbitrageConfig?: RecipeArbitrageConfig | null;
      components?: CompositeComponent[] | null;
    },
  ) => {
    const { tradingConfiguration, ...rest } = input;
    return api.put<IdResponse>(`/api/plutus/strategies/${strategyId}`, {
      ...rest,
      configuration: tradingConfiguration,
    });
  },

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