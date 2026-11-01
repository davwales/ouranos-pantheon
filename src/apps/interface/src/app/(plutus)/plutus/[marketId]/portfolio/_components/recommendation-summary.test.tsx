import { type StrategyRecommendation } from "@/lib/api/plutus";
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { RecommendationSummary } from "./recommendation-summary";

function recommendation(
  overrides: Partial<StrategyRecommendation>,
): StrategyRecommendation {
  return {
    symbolId: "symbol",
    symbolName: "Symbol",
    score: 1,
    suggestedAllocation: 100,
    currentPrice: 10,
    suggestedVolume: 10,
    typicalDailyVolume: 1000,
    limitedBy: "Budget",
    rationale: "",
    ...overrides,
  };
}

describe("RecommendationSummary", () => {
  it("WhenRendered_ShouldSummariseAllocationAndParticipationRate", () => {
    // Arrange
    const recommendations = [
      recommendation({ suggestedAllocation: 400 }),
      recommendation({ suggestedAllocation: 600 }),
    ];

    // Act
    render(
      <RecommendationSummary
        recommendations={recommendations}
        budget={5000}
        volumeParticipationRate={0.025}
      />,
    );

    // Assert
    expect(screen.getByText(/2 picks/)).toHaveTextContent(
      "2 picks · 1.00K of 5.00K allocated · orders capped at 2.5% of volume",
    );
  });

  it("WhenExpectedProfitsPresent_ShouldShowTheirTotal", () => {
    // Arrange
    const recommendations = [
      recommendation({ expectedProfit: 30 }),
      recommendation({ expectedProfit: 20 }),
    ];

    // Act
    render(
      <RecommendationSummary
        recommendations={recommendations}
        budget={1000}
        volumeParticipationRate={0.1}
      />,
    );

    // Assert
    expect(screen.getByText(/est\. profit 50/)).toBeInTheDocument();
  });
});
