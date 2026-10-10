import { beforeEach, describe, expect, it, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import {
  NavBarActionsProvider,
  useNavBarActions,
} from "@/components/shared/nav-bar-actions-context";
import { ApiError } from "@/lib/api-client";
import { plutusApi } from "@/lib/api/plutus";
import type { Symbol } from "@/lib/api/plutus-types";
import SymbolDetail from "./page";

vi.mock("next/navigation", () => ({
  useParams: () => ({ marketId: "market-1", symbolId: "symbol-1" }),
}));

vi.mock("@/lib/api/plutus", () => ({
  SYMBOL_ANALYST_ENDPOINT: "/api/plutus/symbols/assistant/completions/stream",
  plutusApi: {
    getSymbol: vi.fn(),
    getSymbolTrades: vi.fn(),
    getDailySymbolSummary: vi.fn(),
    getAllTrades: vi.fn(),
    getMarketForecasts: vi.fn(),
  },
}));

vi.mock("@/app/(plutus)/plutus/components/price-chart", () => ({
  default: () => null,
}));
vi.mock("@/app/(plutus)/plutus/components/candlestick-chart", () => ({
  default: () => null,
}));
vi.mock("@/app/(plutus)/plutus/components/time-frame-selection", () => ({
  default: () => null,
}));
vi.mock("./_components/signals-section", () => ({
  SignalsSection: () => null,
}));
vi.mock("./_components/forecast-efficacy-view", () => ({
  ForecastEfficacyView: () => null,
}));
vi.mock("./_components/symbol-positions-view", () => ({
  SymbolPositionsView: () => null,
}));

function NavBarActionsOutlet() {
  const { actions } = useNavBarActions();
  return <div data-testid="nav-bar-actions">{actions}</div>;
}

function renderWithNavBar() {
  return render(
    <NavBarActionsProvider>
      <NavBarActionsOutlet />
      <SymbolDetail />
    </NavBarActionsProvider>,
  );
}

function mockSymbol(): Symbol {
  return {
    id: "symbol-1",
    marketId: "market-1",
    name: "Armadyl godsword",
    code: "11802",
    subcode: null,
    createdAt: "2025-01-01T00:00:00.000Z",
    updatedAt: "2025-01-01T00:00:00.000Z",
  };
}

function mockLoadedSymbol() {
  vi.mocked(plutusApi.getSymbol).mockResolvedValue(mockSymbol());
  vi.mocked(plutusApi.getSymbolTrades).mockResolvedValue({
    minPrice: 9800000,
    maxPrice: 10400000,
    averagePrice: 10100000,
    totalSpent: 404000000,
    volume: 40,
    numTransactions: 12,
    trades: [],
  });
  vi.mocked(plutusApi.getDailySymbolSummary).mockResolvedValue({
    averagePrice: 10100000,
    minPrice: 9800000,
    maxPrice: 10400000,
    volume: 40,
  });
  vi.mocked(plutusApi.getAllTrades).mockResolvedValue([]);
  vi.mocked(plutusApi.getMarketForecasts).mockResolvedValue({
    items: [],
    totalCount: 0,
    skip: 0,
    take: 1,
  });
}

describe("SymbolDetail", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("WhenSymbolLoaded_ShouldRenderNameAndStats", async () => {
    // Arrange
    mockLoadedSymbol();

    // Act
    renderWithNavBar();

    // Assert
    expect(await screen.findByRole("heading", { name: "Armadyl godsword" })).toBeInTheDocument();
    expect(screen.getByText("11802")).toBeInTheDocument();
  });

  it("WhenSymbolLoaded_ShouldOfferSymbolAnalystInNavBar", async () => {
    // Arrange
    mockLoadedSymbol();
    renderWithNavBar();

    // Act
    fireEvent.click(await screen.findByRole("button", { name: "Open Symbol Analyst" }));

    // Assert
    expect(await screen.findByRole("heading", { name: "Symbol Analyst" })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Is this a good buy right now?" }),
    ).toBeInTheDocument();
  });

  it("WhenSymbolFailsToLoad_ShouldNotOfferSymbolAnalyst", async () => {
    // Arrange
    mockLoadedSymbol();
    vi.mocked(plutusApi.getSymbol).mockRejectedValue(new ApiError(404, "Not found"));

    // Act
    renderWithNavBar();

    // Assert
    expect(await screen.findByText("Symbol not found")).toBeInTheDocument();
    await waitFor(() =>
      expect(screen.getByTestId("nav-bar-actions")).toBeEmptyDOMElement(),
    );
  });
});
