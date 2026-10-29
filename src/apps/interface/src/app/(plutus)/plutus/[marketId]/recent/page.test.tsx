import { render } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import useInterval from "@/hooks/use-interval";
import RecentMarketTrades from "./page";

vi.mock("next/navigation", () => ({
  useParams: () => ({ marketId: "market-1" }),
}));

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: { getAllTrades: vi.fn(() => new Promise(() => {})) },
}));

vi.mock("@/hooks/use-interval", () => ({ default: vi.fn() }));

vi.mock(
  "@/components/shared/responsive-data-table/responsive-data-table",
  () => ({ default: () => null }),
);

describe("RecentMarketTrades", () => {
  it("WhenRendered_ShouldPollAtTheTradesOutputCacheDuration", () => {
    // Arrange
    const outputCacheMs = 30_000;

    // Act
    render(<RecentMarketTrades />);

    // Assert
    expect(useInterval).toHaveBeenCalledWith(
      expect.any(Function),
      outputCacheMs,
    );
  });
});
