import { type Strategy, plutusApi } from "@/lib/api/plutus";
import { render, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { RecommendationsPanel } from "./recommendations-panel";

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: { getRecommendations: vi.fn() },
}));

vi.mock("./recommendations-toolbar", () => ({
  RecommendationsToolbar: ({
    strategies,
    onStrategyChange,
  }: {
    strategies: Strategy[];
    onStrategyChange: (id: string) => void;
  }) => (
    <div>
      {strategies.map((s) => (
        <button key={s.id} onClick={() => onStrategyChange(s.id)}>
          {s.name}
        </button>
      ))}
    </div>
  ),
}));

function strategy(overrides: Partial<Strategy>): Strategy {
  return {
    id: "strategy",
    marketId: "market-1",
    name: "Strategy",
    isActive: true,
    createdAt: "2026-01-01T00:00:00Z",
    backtestCount: 0,
    ...overrides,
  };
}

function renderPanel(strategies: Strategy[]) {
  return render(
    <RecommendationsPanel
      marketId="market-1"
      strategies={strategies}
      onCreatePosition={vi.fn()}
    />,
  );
}

describe("RecommendationsPanel", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(plutusApi.getRecommendations).mockResolvedValue({
      recommendations: [],
      volumeParticipationRate: 0.1,
    } as unknown as Awaited<ReturnType<typeof plutusApi.getRecommendations>>);
  });

  it("WhenRendered_ShouldGenerateForLatestBacktestedActiveStrategy", async () => {
    // Arrange
    const strategies = [
      strategy({
        id: "older",
        name: "Older",
        lastBacktestAt: "2026-02-01T00:00:00Z",
        lastBacktestBudget: 5000,
      }),
      strategy({
        id: "inactive",
        name: "Inactive",
        isActive: false,
        lastBacktestAt: "2026-04-01T00:00:00Z",
        lastBacktestBudget: 1,
      }),
      strategy({
        id: "latest",
        name: "Latest",
        lastBacktestAt: "2026-03-01T00:00:00Z",
        lastBacktestBudget: 25000,
      }),
    ];

    // Act
    renderPanel(strategies);

    // Assert
    await waitFor(() =>
      expect(plutusApi.getRecommendations).toHaveBeenCalledWith("latest", {
        marketId: "market-1",
        budget: 25000,
      }),
    );
    expect(plutusApi.getRecommendations).toHaveBeenCalledTimes(1);
  });

  it("WhenNoBacktests_ShouldGenerateForFirstActiveStrategyWithDefaultBudget", async () => {
    // Arrange
    const strategies = [
      strategy({ id: "first", name: "First" }),
      strategy({ id: "second", name: "Second" }),
    ];

    // Act
    renderPanel(strategies);

    // Assert
    await waitFor(() =>
      expect(plutusApi.getRecommendations).toHaveBeenCalledWith("first", {
        marketId: "market-1",
        budget: 10000,
      }),
    );
  });

  it("WhenStrategyChanged_ShouldRegenerateWithThatStrategysBudget", async () => {
    // Arrange
    const strategies = [
      strategy({
        id: "latest",
        name: "Latest",
        lastBacktestAt: "2026-03-01T00:00:00Z",
        lastBacktestBudget: 25000,
      }),
      strategy({
        id: "other",
        name: "Other",
        lastBacktestAt: "2026-01-01T00:00:00Z",
        lastBacktestBudget: 7500,
      }),
    ];
    const { getByText } = renderPanel(strategies);
    await waitFor(() =>
      expect(plutusApi.getRecommendations).toHaveBeenCalledTimes(1),
    );

    // Act
    getByText("Other").click();

    // Assert
    await waitFor(() =>
      expect(plutusApi.getRecommendations).toHaveBeenLastCalledWith("other", {
        marketId: "market-1",
        budget: 7500,
      }),
    );
  });
});
