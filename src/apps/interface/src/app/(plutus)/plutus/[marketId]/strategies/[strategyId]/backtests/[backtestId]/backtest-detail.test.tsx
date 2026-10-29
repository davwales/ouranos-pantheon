import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { act, fireEvent, render, screen } from "@testing-library/react";
import { ApiError } from "@/lib/api-client";
import { plutusApi } from "@/lib/api/plutus";
import type { BacktestDetail } from "@/lib/api/plutus-types";
import BacktestDetailPage from "./page";

vi.mock("next/navigation", () => ({
  useParams: () => ({
    marketId: "market-1",
    strategyId: "strategy-1",
    backtestId: "backtest-1",
  }),
  useRouter: () => ({ push: vi.fn() }),
}));

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: {
    getBacktest: vi.fn(),
    getStrategy: vi.fn(),
  },
}));

vi.mock("../../../_components/run-backtest-dialog", () => ({
  RunBacktestDialog: () => null,
}));
vi.mock("../_components/in-progress-backtest-view", () => ({
  InProgressBacktestView: ({ progressPercent }: { progressPercent: number }) => (
    <div>Progress {progressPercent}%</div>
  ),
}));

function runningBacktest(): BacktestDetail {
  return {
    id: "backtest-1",
    strategyId: "strategy-1",
    marketId: "market-1",
    startDate: "2026-01-01T00:00:00Z",
    endDate: "2026-06-30T00:00:00Z",
    budget: 10000,
    kind: "Backtest",
    status: "Running",
    progressPercent: 40,
    progressMessage: null,
    positions: [],
    createdAt: "2026-10-20T00:00:00Z",
    updatedAt: "2026-10-20T00:00:00Z",
  };
}

async function flush() {
  await act(async () => {
    await Promise.resolve();
  });
}

describe("BacktestDetailPage", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.mocked(plutusApi.getStrategy).mockReturnValue(new Promise(() => {}));
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.resetAllMocks();
  });

  it("keeps the loaded backtest when a poll fails", async () => {
    // Arrange
    vi.mocked(plutusApi.getBacktest)
      .mockResolvedValueOnce(runningBacktest())
      .mockRejectedValueOnce(new Error("Gateway timeout"));
    render(<BacktestDetailPage />);
    await flush();
    expect(screen.getByText("Progress 40%")).toBeTruthy();

    // Act
    await act(async () => {
      await vi.advanceTimersByTimeAsync(3000);
    });

    // Assert
    expect(screen.getByText("Progress 40%")).toBeTruthy();
    expect(screen.getByText("Couldn’t refresh this backtest. Retrying…")).toBeTruthy();
    expect(screen.queryByText("Backtest not found")).toBeNull();
  });

  it("recovers on the next successful poll", async () => {
    // Arrange
    vi.mocked(plutusApi.getBacktest)
      .mockResolvedValueOnce(runningBacktest())
      .mockRejectedValueOnce(new Error("Gateway timeout"))
      .mockResolvedValueOnce({ ...runningBacktest(), progressPercent: 60 });
    render(<BacktestDetailPage />);
    await flush();

    // Act
    await act(async () => {
      await vi.advanceTimersByTimeAsync(3000);
    });
    await act(async () => {
      await vi.advanceTimersByTimeAsync(3000);
    });

    // Assert
    expect(screen.getByText("Progress 60%")).toBeTruthy();
    expect(screen.queryByText(/Couldn’t refresh/)).toBeNull();
  });

  it("shows not found when the first load returns 404", async () => {
    // Arrange
    vi.mocked(plutusApi.getBacktest).mockRejectedValueOnce(new ApiError(404, "Not Found"));

    // Act
    render(<BacktestDetailPage />);
    await flush();

    // Assert
    expect(screen.getByText("Backtest not found")).toBeTruthy();
  });

  it("shows a retryable error when the first load fails otherwise", async () => {
    // Arrange
    vi.mocked(plutusApi.getBacktest)
      .mockRejectedValueOnce(new ApiError(500, "Server error"))
      .mockResolvedValueOnce(runningBacktest());
    render(<BacktestDetailPage />);
    await flush();
    expect(screen.queryByText("Backtest not found")).toBeNull();
    expect(screen.getByText("Server error")).toBeTruthy();

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Retry" }));
    await flush();

    // Assert
    expect(screen.getByText("Progress 40%")).toBeTruthy();
  });
});
