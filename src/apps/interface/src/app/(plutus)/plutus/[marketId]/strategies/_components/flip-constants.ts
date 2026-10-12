import { type FlipConfiguration } from "@/lib/api/plutus";

export type FlipSettingKey = Exclude<keyof FlipConfiguration, "mode">;

export const DEFAULT_FLIP_CONFIGURATION: FlipConfiguration = {
  mode: "Flip",
  checkInHours: 8,
  maxOpenOrders: 8,
  lookbackHours: 24,
  bidOffset: 0,
  askOffset: 0,
  minMargin: 0.03,
  maxHoldHours: 72,
  maxPositionPercent: 0.125,
  maxPriceMove: 0.1,
  minPrice: 0,
};

export type FlipField = {
  key: FlipSettingKey;
  label: string;
  hint: string;
  min?: number;
  max?: number;
  step: number;
  isOptimized: boolean;
};

export const FLIP_FIELDS: FlipField[] = [
  {
    key: "checkInHours",
    label: "Check-in Hours",
    hint: "How often you collect fills and re-place orders",
    min: 1,
    step: 1,
    isOptimized: false,
  },
  {
    key: "maxOpenOrders",
    label: "Max Open Orders",
    hint: "Most buy and sell orders open at once",
    min: 1,
    step: 1,
    isOptimized: false,
  },
  {
    key: "minPrice",
    label: "Min Symbol Price",
    hint: "Symbols cheaper than this are never flipped",
    min: 0,
    step: 1,
    isOptimized: false,
  },
  {
    key: "lookbackHours",
    label: "Lookback Hours",
    hint: "History the buy and sell prices are quoted from",
    min: 1,
    step: 1,
    isOptimized: true,
  },
  {
    key: "bidOffset",
    label: "Bid Offset",
    hint: "0 bids the recent sell-side price; higher bids further into the spread",
    step: 0.01,
    isOptimized: true,
  },
  {
    key: "askOffset",
    label: "Ask Offset",
    hint: "0 asks the recent buy-side price; higher asks further into the spread",
    step: 0.01,
    isOptimized: true,
  },
  {
    key: "minMargin",
    label: "Min Margin",
    hint: "Smallest margin after tax, as a fraction of the buy price",
    min: 0,
    step: 0.005,
    isOptimized: true,
  },
  {
    key: "maxHoldHours",
    label: "Max Hold Hours",
    hint: "Unsold holdings older than this are re-listed at the sell-side price",
    min: 1,
    step: 1,
    isOptimized: true,
  },
  {
    key: "maxPositionPercent",
    label: "Max Position %",
    hint: "Largest share of equity committed to one symbol (0-1)",
    min: 0.01,
    max: 1,
    step: 0.01,
    isOptimized: true,
  },
  {
    key: "maxPriceMove",
    label: "Max Price Move",
    hint: "Skip symbols more than this far from their 7-day price (0 disables)",
    min: 0,
    step: 0.01,
    isOptimized: true,
  },
];
