import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { plutusApi } from "@/lib/api/plutus";
import { RunBacktestForm } from "./run-backtest-form";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: { runBacktest: vi.fn() },
}));

describe("RunBacktestForm", () => {
  it("WhenBacktestStarted_ShouldNavigateToTheNewBacktest", async () => {
    // Arrange
    vi.mocked(plutusApi.runBacktest).mockResolvedValue({
      backtestId: "backtest-1",
    });
    const onDone = vi.fn();
    render(
      <RunBacktestForm strategyId="strategy-1" marketId="market-1" onDone={onDone} />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Run Backtest" }));

    // Assert
    await waitFor(() =>
      expect(push).toHaveBeenCalledWith(
        "/plutus/market-1/strategies/strategy-1/backtests/backtest-1",
      ),
    );
    expect(onDone).toHaveBeenCalledOnce();
  });
});
