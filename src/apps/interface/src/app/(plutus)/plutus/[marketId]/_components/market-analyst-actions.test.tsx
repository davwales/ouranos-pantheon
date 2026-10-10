import { MarketAnalystActions } from "@/app/(plutus)/plutus/[marketId]/_components/market-analyst-actions";
import {
  NavBarActionsProvider,
  useNavBarActions,
} from "@/components/shared/nav-bar-actions-context";
import { usePlutusStore } from "@/stores/plutus-store";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/lib/api/plutus", () => ({
  MARKET_ANALYST_ENDPOINT: "/api/plutus/markets/assistant/completions/stream",
}));

function NavBarActionsOutlet() {
  const { actions } = useNavBarActions();
  return <div data-testid="nav-bar-actions">{actions}</div>;
}

describe("MarketAnalystActions", () => {
  it("WhenMounted_ShouldOfferMarketAnalystInNavBar", async () => {
    // Arrange
    usePlutusStore.setState({ timeFrameKey: "OneDay" });
    render(
      <NavBarActionsProvider>
        <NavBarActionsOutlet />
        <MarketAnalystActions marketId="market-1" />
      </NavBarActionsProvider>,
    );

    // Act
    fireEvent.click(await screen.findByRole("button", { name: "Open Market Analyst" }));

    // Assert
    expect(await screen.findByRole("heading", { name: "Market Analyst" })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "What are the best flips right now?" }),
    ).toBeInTheDocument();
  });

  it("WhenUnmounted_ShouldClearTheNavBarAction", () => {
    // Arrange
    const { rerender } = render(
      <NavBarActionsProvider>
        <NavBarActionsOutlet />
        <MarketAnalystActions marketId="market-1" />
      </NavBarActionsProvider>,
    );

    // Act
    rerender(
      <NavBarActionsProvider>
        <NavBarActionsOutlet />
      </NavBarActionsProvider>,
    );

    // Assert
    expect(screen.getByTestId("nav-bar-actions")).toBeEmptyDOMElement();
  });
});
