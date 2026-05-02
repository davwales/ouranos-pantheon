import { StrategyType } from "@/lib/api/plutus";
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
