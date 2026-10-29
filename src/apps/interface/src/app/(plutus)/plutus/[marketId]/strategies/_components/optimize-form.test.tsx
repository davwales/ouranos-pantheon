import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { plutusApi } from "@/lib/api/plutus";
import { OptimizeForm } from "./optimize-form";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: { optimizeStrategy: vi.fn() },
}));

describe("OptimizeForm", () => {
  it("WhenOptimizationStarted_ShouldNavigateToTheNewBacktest", async () => {
    // Arrange
    vi.mocked(plutusApi.optimizeStrategy).mockResolvedValue({
      backtestId: "backtest-1",
    });
    const onDone = vi.fn();
    render(
      <OptimizeForm
        strategyId="strategy-1"
        marketId="market-1"
        configuration={{ mode: "Swing", inputWeights: [] }}
        onDone={onDone}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Start Optimization" }));

    // Assert
    await waitFor(() =>
      expect(push).toHaveBeenCalledWith(
        "/plutus/market-1/strategies/strategy-1/backtests/backtest-1",
      ),
    );
    expect(onDone).toHaveBeenCalledOnce();
  });
});
