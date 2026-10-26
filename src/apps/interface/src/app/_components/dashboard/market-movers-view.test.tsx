import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { type Market } from "@/lib/api/plutus";
import { MarketMoversView } from "./market-movers-view";

const plutusApi = vi.hoisted(() => ({ getMarketTrades: vi.fn() }));
vi.mock("@/lib/api/plutus", () => ({ plutusApi }));

const markets = [
  { id: "m1", name: "OSRS" } as Market,
  { id: "m2", name: "Stocks" } as Market,
];

describe("MarketMoversView", () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it("shows the top symbols by daily gain per market, skipping empty markets", async () => {
    plutusApi.getMarketTrades.mockImplementation(async (marketId: string) => ({
      items:
        marketId === "m1"
          ? [
              {
                symbolId: "s1",
                symbolName: "Dragon bolts",
                averagePrice: 1500,
                totalVolume: 2000,
                totalGain: 3_000_000,
                roi: 0.125,
              },
            ]
          : [],
      totalCount: 0,
      skip: 0,
      take: 3,
    }));

    render(<MarketMoversView markets={markets} />);

    expect(await screen.findByText("Dragon bolts")).toBeInTheDocument();
    expect(screen.getByText("12.5% ROI")).toBeInTheDocument();
    expect(screen.queryByText("Stocks")).not.toBeInTheDocument();
    expect(screen.getByRole("link")).toHaveAttribute("href", "/plutus/m1/s1");
    expect(plutusApi.getMarketTrades).toHaveBeenCalledWith("m1", "OneDay", {
      sortField: "TotalGain",
      sortDirection: "desc",
      take: 3,
    });
  });
});
