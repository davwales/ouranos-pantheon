import { type Position, type StrategyRecommendation } from "@/lib/api/plutus";
import { describe, expect, it } from "vitest";
import {
  defaultsForSellOf,
  defaultsFromRecommendation,
} from "./position-defaults";

function recommendation(
  overrides: Partial<StrategyRecommendation>,
): StrategyRecommendation {
  return {
    symbolId: "symbol-1",
    symbolName: "Symbol",
    score: 1,
    suggestedAllocation: 1000,
    currentPrice: 95,
    suggestedVolume: 10,
    typicalDailyVolume: 500,
    limitedBy: "Budget",
    rationale: "",
    ...overrides,
  };
}

function position(overrides: Partial<Position>): Position {
  return {
    id: "position-1",
    side: "Buy",
    status: "Bought",
    marketId: "market-1",
    symbolId: "symbol-1",
    symbolName: "Symbol",
    cost: 100,
    quantity: 7,
    targetPrice: null,
    linkedBuyPositionId: null,
    strategyId: "strategy-1",
    notes: null,
    createdAt: "2026-01-01T00:00:00Z",
    updatedAt: "2026-01-01T00:00:00Z",
    ...overrides,
  };
}

describe("defaultsFromRecommendation", () => {
  it("WhenFlipRecommendation_ShouldUseBuyAndSellPrices", () => {
    // Arrange
    const rec = recommendation({ buyPrice: 100, sellPrice: 120 });

    // Act
    const defaults = defaultsFromRecommendation(rec, "strategy-1");

    // Assert
    expect(defaults).toEqual({
      cost: 100,
      quantity: 10,
      targetPrice: 120,
      strategyId: "strategy-1",
    });
  });

  it("WhenSwingRecommendation_ShouldUseCurrentPriceWithoutTarget", () => {
    // Arrange
    const rec = recommendation({});

    // Act
    const defaults = defaultsFromRecommendation(rec, "strategy-1");

    // Assert
    expect(defaults).toEqual({
      cost: 95,
      quantity: 10,
      targetPrice: null,
      strategyId: "strategy-1",
    });
  });
});

describe("defaultsForSellOf", () => {
  it("WhenBuyHasTarget_ShouldUseTargetAndBuyQuantity", () => {
    // Arrange
    const buy = position({ targetPrice: 130 });

    // Act
    const defaults = defaultsForSellOf(buy);

    // Assert
    expect(defaults).toEqual({
      cost: 130,
      quantity: 7,
      strategyId: "strategy-1",
    });
  });

  it("WhenBuyHasNoTarget_ShouldStillUseBuyQuantity", () => {
    // Arrange
    const buy = position({ targetPrice: null });

    // Act
    const defaults = defaultsForSellOf(buy);

    // Assert
    expect(defaults.cost).toBeNull();
    expect(defaults.quantity).toBe(7);
  });
});
