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

export interface HeatmapCell {
  dayOfWeek: number;
  hour: number;
  totalTrades: number;
  percentage: number;
}

export interface GetVolumeHeatmapResponse {
  rows: HeatmapCell[];
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

export interface SignalHistoryPoint {
  value: number;
  computedAt: string;
}

export interface SignalHistoryResponse {
  type: string;
  label: string;
  description: string;
  intents: string[];
  currentValue: number;
  direction: string;
  strength: string;
  history: SignalHistoryPoint[];
}

export interface GetSymbolSignalHistoryResponse {
  symbolId: string;
  symbolName: string;
  signals: SignalHistoryResponse[];
  summary: SignalSummary;
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

export interface TradingConfiguration {
  maxPositions: number;
  maxPositionPercent: number;
  holdPeriodDays: number;
}

export interface SignalWeightedConfig {
  buyThreshold?: number | null;
  sellThreshold?: number | null;
  taxAdjustedRoiWeight?: number | null;
  volumeAnomalyWeight?: number | null;
  trendMomentumWeight?: number | null;
  bollingerBandsWeight?: number | null;
  rsiWeight?: number | null;
  movingAverageCrossoverWeight?: number | null;
  priceVelocityWeight?: number | null;
}

export interface ForecastMomentumConfig {
  forecastMovementThreshold?: number | null;
  forecastHorizonDays?: number | null;
}

export interface MeanReversionConfig {
  deviationMultiplier?: number | null;
  meanTimeFrameValue?: number | null;
}

export interface RecipeArbitrageConfig {
  minMarginPercent?: number | null;
}

export type StrategyConfigBundle = {
  tradingConfiguration: TradingConfiguration;
  signalWeightedConfig?: SignalWeightedConfig | null;
  forecastMomentumConfig?: ForecastMomentumConfig | null;
  meanReversionConfig?: MeanReversionConfig | null;
  recipeArbitrageConfig?: RecipeArbitrageConfig | null;
  components?: CompositeComponent[] | null;
};

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
  tradingConfiguration: TradingConfiguration;
  signalWeightedConfig?: SignalWeightedConfig | null;
  forecastMomentumConfig?: ForecastMomentumConfig | null;
  meanReversionConfig?: MeanReversionConfig | null;
  recipeArbitrageConfig?: RecipeArbitrageConfig | null;
  components?: CompositeComponent[] | null;
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
  optimizedConfiguration?: TradingConfiguration | null;
  optimizedSignalWeightedConfig?: SignalWeightedConfig | null;
  optimizedForecastMomentumConfig?: ForecastMomentumConfig | null;
  optimizedMeanReversionConfig?: MeanReversionConfig | null;
  optimizedRecipeArbitrageConfig?: RecipeArbitrageConfig | null;
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
  positions: BacktestPosition[];
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

export interface ForecastEfficacyRow {
  symbolId: string;
  symbolName: string;
  marketId: string;
  modelName: string;
  horizonDays: number;
  evaluatedCount: number;
  meanAbsoluteError: number | null;
  meanAbsolutePercentageError: number | null;
  meanBias: number | null;
  firstGeneratedAt: string | null;
  lastGeneratedAt: string | null;
}

export interface PageParams {
  skip?: number;
  take?: number;
  sortField?: string;
  sortDirection?: string;
  filter?: string[];
  [key: string]: string | number | boolean | string[] | undefined;
}