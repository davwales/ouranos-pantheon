import { type Position, plutusApi } from "@/lib/api/plutus";
import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { LinkPositionDialog } from "./link-position-dialog";

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: { getAllPositions: vi.fn(), linkPosition: vi.fn() },
}));

vi.mock("@/components/shared/responsive-dialog/responsive-dialog", () => ({
  ResponsiveDialog: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
}));

const sellPosition: Position = {
  id: "sell-1",
  side: "Sell",
  status: "Pending",
  marketId: "market-1",
  symbolId: "symbol-1",
  symbolName: "Symbol",
  cost: 130,
  quantity: 3,
  targetPrice: null,
  linkedBuyPositionId: null,
  strategyId: null,
  notes: null,
  createdAt: "2026-10-01T00:00:00Z",
  updatedAt: "2026-10-01T00:00:00Z",
};

describe("LinkPositionDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(plutusApi.getAllPositions).mockResolvedValue({
      items: [],
      totalCount: 0,
      skip: 0,
      take: 100,
    });
  });

  it("WhenOpened_ShouldRequestOnlyBoughtBuysOfTheSymbolWithEnoughUnsold", async () => {
    // Arrange & Act
    render(
      <LinkPositionDialog
        sellPosition={sellPosition}
        marketId="market-1"
        open
        onOpenChange={vi.fn()}
      />,
    );

    // Assert
    await waitFor(() =>
      expect(plutusApi.getAllPositions).toHaveBeenCalledWith(
        "market-1",
        expect.objectContaining({
          filter: [
            "side:eq:Buy",
            "status:eq:Bought",
            "symbolId:eq:symbol-1",
            "unsoldQuantity:gte:3",
          ],
        }),
      ),
    );
    expect(
      await screen.findByText("No bought Symbol positions with 3 unsold."),
    ).toBeTruthy();
  });
});
