import { type StrategyType } from "@/lib/api/plutus";
import { Activity, Calendar, Clock, Gauge } from "lucide-react";

export const strategyTypeLabels: Record<StrategyType, string> = {
  SignalWeighted: "Signal Weighted",
  ForecastMomentum: "Forecast Momentum",
  MeanReversion: "Mean Reversion",
  RecipeArbitrage: "Recipe Arbitrage",
  Composite: "Composite",
};

export const signalTypeLabels: Record<string, string> = {
  TaxAdjustedRoi: "Tax Adjusted ROI",
  VolumeAnomaly: "Volume Anomaly",
  TrendMomentum: "Trend Momentum",
  BollingerBands: "Bollinger Bands",
  Rsi: "RSI",
  MovingAverageCrossover: "Moving Average Crossover",
  PriceVelocity: "Price Velocity",
};

export type SignalWeightKey =
  | "taxAdjustedRoiWeight"
  | "volumeAnomalyWeight"
  | "trendMomentumWeight"
  | "bollingerBandsWeight"
  | "rsiWeight"
  | "movingAverageCrossoverWeight"
  | "priceVelocityWeight";

export const signalWeightFields: Array<{
  label: string;
  key: SignalWeightKey;
}> = [
  { label: signalTypeLabels.TaxAdjustedRoi, key: "taxAdjustedRoiWeight" },
  { label: signalTypeLabels.VolumeAnomaly, key: "volumeAnomalyWeight" },
  { label: signalTypeLabels.TrendMomentum, key: "trendMomentumWeight" },
  { label: signalTypeLabels.BollingerBands, key: "bollingerBandsWeight" },
  { label: signalTypeLabels.Rsi, key: "rsiWeight" },
  {
    label: signalTypeLabels.MovingAverageCrossover,
    key: "movingAverageCrossoverWeight",
  },
  { label: signalTypeLabels.PriceVelocity, key: "priceVelocityWeight" },
];

export function typeIcon(type: StrategyType) {
  switch (type) {
    case "SignalWeighted":
      return <Gauge className="size-5" />;
    case "ForecastMomentum":
      return <Activity className="size-5" />;
    case "MeanReversion":
      return <Clock className="size-5" />;
    case "RecipeArbitrage":
      return <Calendar className="size-5" />;
    default:
      return <Gauge className="size-5" />;
  }
}
