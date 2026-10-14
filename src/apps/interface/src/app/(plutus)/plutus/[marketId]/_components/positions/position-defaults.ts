import { type Position, type StrategyRecommendation } from "@/lib/api/plutus";

export type PositionDefaults = {
  cost?: number | null;
  quantity?: number | null;
  targetPrice?: number | null;
  strategyId?: string | null;
};

export function defaultsFromRecommendation(
  recommendation: StrategyRecommendation,
  strategyId: string,
): PositionDefaults {
  return {
    cost: recommendation.buyPrice ?? recommendation.currentPrice,
    quantity: recommendation.suggestedVolume,
    targetPrice: recommendation.sellPrice ?? null,
    strategyId,
  };
}

export function defaultsForSellOf(buy: Position): PositionDefaults {
  return {
    cost: buy.targetPrice,
    quantity: buy.quantity,
    strategyId: buy.strategyId,
  };
}
