import { type InputKind, type SwingConfiguration } from "@/lib/api/plutus";

export type ThresholdKey = keyof Pick<
  SwingConfiguration,
  "buyThreshold" | "sellThreshold"
>;

export const INPUT_KIND_LABELS: Record<InputKind, string> = {
  SignalTaxAdjustedRoi: "Tax-Adjusted ROI",
  SignalVolumeAnomaly: "Volume Anomaly",
  SignalTrendMomentum: "Trend Momentum",
  SignalBollingerBands: "Bollinger Bands",
  SignalRsi: "Relative Strength Index",
  SignalMovingAverageCrossover: "Moving Average Crossover",
  SignalPriceVelocity: "Price Velocity",
};

export const INPUT_KINDS: InputKind[] = [
  "SignalTaxAdjustedRoi",
  "SignalVolumeAnomaly",
  "SignalTrendMomentum",
  "SignalBollingerBands",
  "SignalRsi",
  "SignalMovingAverageCrossover",
  "SignalPriceVelocity",
];

export const THRESHOLD_FIELDS: ReadonlyArray<{
  key: ThresholdKey;
  label: string;
  hint?: string;
  placeholder?: string;
  min?: number;
  max?: number;
  step?: number;
}> = [
  {
    key: "buyThreshold",
    label: "Buy Threshold",
    hint: "Score above which to buy",
    placeholder: "0",
    min: 0,
    max: 1,
    step: 0.01,
  },
  {
    key: "sellThreshold",
    label: "Sell Threshold",
    hint: "Score below which to sell",
    placeholder: "None (hold period only)",
    min: -1,
    max: 0,
    step: 0.01,
  },
];
