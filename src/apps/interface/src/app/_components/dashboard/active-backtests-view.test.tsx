import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { type Market } from "@/lib/api/plutus";
import { ActiveBacktestsView } from "./active-backtests-view";

const plutusApi = vi.hoisted(() => ({ getActiveBacktests: vi.fn() }));
vi.mock("@/lib/api/plutus", () => ({ plutusApi }));

const markets = [{ id: "m1", name: "OSRS" } as Market];

describe("ActiveBacktestsView", () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it("lists running backtests with progress and a link to the run", async () => {
    plutusApi.getActiveBacktests.mockResolvedValue({
      items: [
        {
          id: "b1",
          strategyId: "s1",
          strategyName: "Swing Alpha",
          marketId: "m1",
          kind: "Optimization",
          status: "Running",
          progressPercent: 40,
          progressMessage: "Generation 4 of 10",
          createdAt: "2026-10-25T00:00:00Z",
        },
      ],
      totalCount: 1,
      skip: 0,
      take: 5,
    });

    render(<ActiveBacktestsView markets={markets} />);

    expect(await screen.findByText("Swing Alpha")).toBeInTheDocument();
    expect(screen.getByText("Running Backtests (1)")).toBeInTheDocument();
    expect(
      screen.getByText("OSRS · Optimization · Generation 4 of 10"),
    ).toBeInTheDocument();
    expect(screen.getByText("40%")).toBeInTheDocument();
    expect(screen.getByRole("link")).toHaveAttribute(
      "href",
      "/plutus/m1/strategies/s1/backtests/b1",
    );
    expect(plutusApi.getActiveBacktests).toHaveBeenCalledWith({ take: 5 });
  });

  it("shows an empty state when nothing is running", async () => {
    plutusApi.getActiveBacktests.mockResolvedValue({
      items: [],
      totalCount: 0,
      skip: 0,
      take: 5,
    });

    render(<ActiveBacktestsView markets={markets} />);

    expect(
      await screen.findByText("No backtests or optimizations are running."),
    ).toBeInTheDocument();
  });
});
