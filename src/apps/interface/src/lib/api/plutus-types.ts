import { type TimeFrameKey } from "@/app/(plutus)/plutus/constants/time-frames";

export type Market = {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  taxes: unknown;
  isForecastingEnabled: boolean;
};

export type Symbol = {
  id: string;
  marketId: string;
  name: string;
  code: string;
  subcode?: string | null;
  createdAt: string;
  updatedAt: string;
};

export type Trade = {
  id: string;
  symbolId: string;
  marketId: string;
  symbolName: string;
  symbolCode: string;
  price: number;
  volume: number;
  timestamp: string;
};

export type GetMarketTradesRow = {
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
};

export type GetRecipeTradesRow = {
  recipeId: string;
  recipeName: string;
  latestBuyPrice: number;
  latestSellPrice: number;
  latestMargin: number;
  averageBuyPrice: number;
  averageSellPrice: number;
  averageMargin: number;
};

export type TradeBucket = {
  date: string;
  maxPrice: number;
  minPrice: number;
  numTransactions: number;
  price: number;
  totalSpent: number;
  volume: number;
  openPrice: number;
  closePrice: number;
};

export type GetSymbolTradesResponse = {
  minPrice: number;
  maxPrice: number;
  averagePrice: number;
  totalSpent: number;
  volume: number;
  numTransactions: number;
  trades: TradeBucket[];
};

export type MarketOverviewBucket = {
  date: string;
  numTransactions: number;
  price: number;
  totalSpent: number;
  volume: number;
};

export type GetMarketOverviewResponse = {
  averagePrice: number;
  totalSpent: number;
  volume: number;
  numTransactions: number;
  trades: MarketOverviewBucket[];
};

export type HeatmapCell = {
  dayOfWeek: number;
  hour: number;
  totalTrades: number;
  percentage: number;
};

export type GetVolumeHeatmapResponse = {
  rows: HeatmapCell[];
};

export type GetDailySymbolSummaryResponse = {
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
  volume: number;
};

export type ForecastPoint = {
  averagePrice: number;
  minPrice: number;
  maxPrice: number;
  volume: number;
};

export type ForecastPrediction = {
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
};

export type GetMarketForecastRow = {
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
};

export type Recipe = {
  id: string;
  name: string;
  cost: number;
  marketId: string;
  inputs: RecipeComponent[];
  outputs: RecipeComponent[];
};

export type RecipeComponent = {
  symbolId: string;
  name: string;
  quantity: number;
  latestPrice?: number | null;
  averagePrice?: number | null;
  totalValue?: number | null;
  volume?: number | null;
};

export type SignalResponse = {
  type: string;
  label: string;
  description: string;
  intents: string[];
  value: number;
  direction: string;
  strength: string;
};

export type SignalSummary = {
  aggregatedScore: number;
  bullishCount: number;
  bearishCount: number;
  neutralCount: number;
  isFlipFavourable: boolean;
  isMerchFavourable: boolean;
};

export type SignalHistoryPoint = {
  value: number;
  computedAt: string;
};

export type SignalHistoryResponse = {
  type: string;
  label: string;
  description: string;
  intents: string[];
  currentValue: number;
  direction: string;
  strength: string;
  history: SignalHistoryPoint[];
};

export type GetSymbolSignalHistoryResponse = {
  symbolId: string;
  symbolName: string;
  signals: SignalHistoryResponse[];
  summary: SignalSummary;
};

export type GetSymbolSignalsResponse = {
  signals: SignalResponse[];
  summary: SignalSummary;
};

export type GetSignalRankingsRow = {
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
};

export type SymbolGroup = {
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
};

export type SymbolGroupDetail = {
  id: string;
  marketId: string;
  name: string;
  description?: string | null;
  symbols: SymbolGroupSymbol[];
};

export type SymbolGroupSymbol = {
  symbolId: string;
  code: string;
  subcode?: string | null;
  name: string;
  addedAt: string;
  volume?: number | null;
  gain?: number | null;
  roi?: number | null;
  signalScore?: number | null;
};

export type IdResponse = {
  id: string;
};

export type BacktestStatus =
  | "Pending"
  | "Running"
  | "Completed"
  | "Failed"
  | "Cancelled";

export type BacktestKind = "Backtest" | "Optimization";

export type InputKind =
  | "SignalTaxAdjustedRoi"
  | "SignalVolumeAnomaly"
  | "SignalTrendMomentum"
  | "SignalBollingerBands"
  | "SignalRsi"
  | "SignalMovingAverageCrossover"
  | "SignalPriceVelocity";

export type InputWeight = {
  kind: InputKind;
  weight: number;
};

export type SwingConfiguration = {
  mode: "Swing";
  inputWeights: InputWeight[];
  buyThreshold?: number | null;
  sellThreshold?: number | null;
  maxPositions?: number | null;
  maxPositionPercent?: number | null;
  holdPeriodDays?: number | null;
  lockedParameters?: string[];
};

export type FlipConfiguration = {
  mode: "Flip";
  checkInHours: number;
  maxOpenOrders: number;
  lookbackHours: number;
  bidOffset: number;
  askOffset: number;
  minMargin: number;
  maxHoldHours: number;
  maxPositionPercent: number;
  maxPriceMove: number;
  minPrice: number;
  lockedParameters?: string[];
};

export type StrategyConfiguration = SwingConfiguration | FlipConfiguration;

export type StrategyMode = StrategyConfiguration["mode"];

export type Strategy = {
  id: string;
  marketId: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  createdAt: string;
  backtestCount: number;
  lastBacktestReturn?: number | null;
  lastBacktestWinRate?: number | null;
  lastBacktestBudget?: number | null;
  lastBacktestAt?: string | null;
};

export type StrategyDetail = {
  id: string;
  marketId: string;
  name: string;
  description?: string | null;
  configuration: StrategyConfiguration;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type CreateStrategyRequest = {
  marketId: string;
  name: string;
  description?: string | null;
  configuration: StrategyConfiguration;
};

export type UpdateStrategyRequest = {
  name: string;
  description?: string | null;
  configuration: StrategyConfiguration;
};

export type BacktestSummary = {
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
};

export type ActiveBacktest = {
  id: string;
  strategyId: string;
  strategyName: string;
  marketId: string;
  kind: BacktestKind;
  status: BacktestStatus;
  progressPercent: number;
  progressMessage?: string | null;
  createdAt: string;
};

export type BacktestPosition = {
  symbolId: string;
  symbolName: string;
  entryPrice: number;
  exitPrice: number;
  volume: number;
  profitLoss: number;
  returnPercent: number;
  entryTime: string;
  exitTime: string;
};

export type BacktestResults = {
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
  turnoverRate: number;
  isValidated: boolean;
  outSampleResults: BacktestResults | null;
  optimizedConfiguration?: StrategyConfiguration | null;
};

export type BacktestDetail = {
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
};

export type RunBacktestResponse = {
  backtestId: string;
};

export type CancelBacktestResponse = {
  backtestId: string;
  status: BacktestStatus;
};

export type RestartBacktestResponse = {
  backtestId: string;
  status: BacktestStatus;
};

export type StrategyRecommendation = {
  symbolId: string;
  symbolName: string;
  symbolSubcode?: string | null;
  score: number;
  suggestedAllocation: number;
  currentPrice: number;
  suggestedVolume: number;
  typicalDailyVolume: number;
  limitedBy: PositionLimit;
  rationale: string;
  buyPrice?: number | null;
  sellPrice?: number | null;
  netMargin?: number | null;
  expectedProfit?: number | null;
};

export type PositionLimit = "Budget" | "Liquidity" | "BuyLimit";

export type GetRecommendationsResponse = {
  recommendations: StrategyRecommendation[];
};

export type PositionSide = "Buy" | "Sell";

export type PositionStatus =
  | "Pending"
  | "DidNotBuy"
  | "Bought"
  | "DidNotSell"
  | "Sold";

export type Position = {
  id: string;
  side: PositionSide;
  status: PositionStatus;
  marketId: string;
  symbolId: string;
  symbolName: string;
  cost: number;
  quantity: number;
  targetPrice: number | null;
  linkedBuyPositionId: string | null;
  strategyId: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
};

export type ClosePositionResponse = {
  positionId: string;
  status: PositionStatus;
};

export type ForecastEfficacyRow = {
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
};

export type PageParams = {
  skip?: number;
  take?: number;
  sortField?: string;
  sortDirection?: string;
  filter?: string[];
  [key: string]: string | number | boolean | string[] | undefined;
};

export type SymbolAnalystContext = {
  marketId: string;
  symbolId: string;
  timeFrame: TimeFrameKey;
};

export type ExplorerView = {
  filter?: string[];
  sortField?: string;
  sortDirection?: string;
  skip?: number;
  take?: number;
};

export type MarketAnalystContext = {
  marketId: string;
  timeFrame: TimeFrameKey;
  view?: ExplorerView;
};

export type ExplorerCondition = {
  field: string;
  operator: string;
  value: string;
};

export type SetExplorerViewAction = {
  conditions: ExplorerCondition[];
  sortField: string | null;
  descending: boolean;
};
