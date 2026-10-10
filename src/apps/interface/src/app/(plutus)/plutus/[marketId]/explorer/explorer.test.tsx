import {
  NavBarActionsProvider,
  useNavBarActions,
} from "@/components/shared/nav-bar-actions-context";
import { streamAssistant } from "@/lib/api/assistant";
import { plutusApi } from "@/lib/api/plutus";
import { usePlutusStore } from "@/stores/plutus-store";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import MarketDetail from "./page";

vi.mock("next/navigation", () => ({
  useParams: () => ({ marketId: "market-1" }),
}));

vi.mock("@/lib/api/plutus", () => ({
  MARKET_ANALYST_ENDPOINT: "/api/plutus/markets/assistant/completions/stream",
  plutusApi: { getMarketTrades: vi.fn() },
}));

vi.mock("@/lib/api/assistant", () => ({
  streamAssistant: vi.fn(),
}));

vi.mock("@/app/(plutus)/plutus/components/time-frame-selection", () => ({
  default: () => null,
}));

function NavBarActionsOutlet() {
  const { actions } = useNavBarActions();
  return <div data-testid="nav-bar-actions">{actions}</div>;
}

describe("MarketDetail (Explorer)", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(plutusApi.getMarketTrades).mockResolvedValue({
      items: [],
      totalCount: 0,
      skip: 0,
      take: 10,
    });
    vi.mocked(streamAssistant).mockImplementation(async function* () {
      yield { $type: "done" as const };
    });
    usePlutusStore.setState((state) => ({
      timeFrameKey: "OneDay",
      explorerTableState: {
        ...state.explorerTableState,
        pagination: { pageSize: 10, skip: 20, take: 10 },
        filter: {
          logic: "and",
          items: [{ field: "symbolName", operator: "contains", value: "rune" }],
        },
        sort: { roi: "DESC" },
      },
    }));
  });

  it("WhenRendered_ShouldOfferMarketAnalystWithTheCurrentView", async () => {
    // Arrange
    render(
      <NavBarActionsProvider>
        <NavBarActionsOutlet />
        <MarketDetail />
      </NavBarActionsProvider>,
    );

    // Act
    fireEvent.click(await screen.findByRole("button", { name: "Open Market Analyst" }));
    fireEvent.click(
      await screen.findByRole("button", { name: "Which of these is the best opportunity?" }),
    );

    // Assert
    await waitFor(() =>
      expect(streamAssistant).toHaveBeenCalledWith(
        "/api/plutus/markets/assistant/completions/stream",
        expect.objectContaining({
          context: {
            marketId: "market-1",
            timeFrame: "OneDay",
            filter: ["symbolName:like:rune"],
            sortField: "roi",
            sortDirection: "desc",
            skip: 20,
            take: 10,
          },
        }),
        expect.any(AbortSignal),
      ),
    );
  });
});
