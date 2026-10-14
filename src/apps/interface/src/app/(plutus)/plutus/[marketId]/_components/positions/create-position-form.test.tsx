import { plutusApi } from "@/lib/api/plutus";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { CreatePositionForm } from "./create-position-form";

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: { createPosition: vi.fn(), linkPosition: vi.fn() },
}));

describe("CreatePositionForm", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(plutusApi.createPosition).mockResolvedValue({ id: "new" });
  });

  it("WhenDefaultsProvided_ShouldSubmitThemWithBuy", async () => {
    // Arrange
    render(
      <CreatePositionForm
        marketId="market-1"
        onDone={vi.fn()}
        defaultSymbolId="symbol-1"
        defaultSymbolName="Symbol"
        defaults={{
          cost: 100,
          quantity: 10,
          targetPrice: 120,
          strategyId: "strategy-1",
        }}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Create Position" }));

    // Assert
    await waitFor(() =>
      expect(plutusApi.createPosition).toHaveBeenCalledWith({
        side: "Buy",
        marketId: "market-1",
        symbolId: "symbol-1",
        cost: 100,
        quantity: 10,
        strategyId: "strategy-1",
        notes: null,
        targetPrice: 120,
      }),
    );
  });

  it("WhenCreatingLinkedSell_ShouldPrefillQuantityAndOmitTarget", async () => {
    // Arrange
    render(
      <CreatePositionForm
        marketId="market-1"
        onDone={vi.fn()}
        linkedBuyPositionId="buy-1"
        defaultSymbolId="symbol-1"
        defaultSymbolName="Symbol"
        defaults={{ cost: 130, quantity: 7, strategyId: "strategy-1" }}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Create Position" }));

    // Assert
    await waitFor(() =>
      expect(plutusApi.createPosition).toHaveBeenCalledWith(
        expect.objectContaining({
          side: "Sell",
          cost: 130,
          quantity: 7,
          targetPrice: null,
        }),
      ),
    );
    expect(screen.queryByText("Target sell price")).toBeNull();
    await waitFor(() =>
      expect(plutusApi.linkPosition).toHaveBeenCalledWith("new", "buy-1"),
    );
  });
});
