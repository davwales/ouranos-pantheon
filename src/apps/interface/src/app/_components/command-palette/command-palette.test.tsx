import { act, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { CommandPalette } from "./command-palette";

const push = vi.hoisted(() => vi.fn());
const setTheme = vi.hoisted(() => vi.fn());
const plutusApi = vi.hoisted(() => ({
  getAllMarkets: vi.fn(),
  getAllSymbols: vi.fn(),
  getAllSymbolGroups: vi.fn(),
  getAllStrategies: vi.fn(),
  getAllRecipes: vi.fn(),
}));
const hestiaApi = vi.hoisted(() => ({ getAllRecipes: vi.fn() }));
const hermesApi = vi.hoisted(() => ({ getAllConversations: vi.fn() }));

vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));
vi.mock("next-themes", () => ({ useTheme: () => ({ setTheme }) }));
vi.mock("@/lib/api/plutus", () => ({ plutusApi }));
vi.mock("@/lib/api/hestia", () => ({ hestiaApi }));
vi.mock("@/lib/api/hermes", () => ({ hermesApi }));

const emptyPage = { items: [], totalCount: 0, skip: 0, take: 5 };

function pressShortcut() {
  act(() => {
    window.dispatchEvent(
      new KeyboardEvent("keydown", { key: "k", ctrlKey: true }),
    );
  });
}

describe("CommandPalette", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    plutusApi.getAllMarkets.mockResolvedValue([{ id: "m1", name: "OSRS" }]);
    plutusApi.getAllSymbols.mockResolvedValue({
      ...emptyPage,
      items: [{ id: "s1", marketId: "m1", name: "Dragon bolts" }],
    });
    plutusApi.getAllSymbolGroups.mockResolvedValue(emptyPage);
    plutusApi.getAllStrategies.mockResolvedValue(emptyPage);
    plutusApi.getAllRecipes.mockResolvedValue(emptyPage);
    hestiaApi.getAllRecipes.mockResolvedValue(emptyPage);
    hermesApi.getAllConversations.mockResolvedValue([]);
  });

  it("does not load data until it is opened", () => {
    render(<CommandPalette />);

    expect(plutusApi.getAllMarkets).not.toHaveBeenCalled();
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("opens from the keyboard shortcut", async () => {
    render(<CommandPalette />);

    pressShortcut();

    expect(await screen.findByRole("dialog")).toBeInTheDocument();
    expect(await screen.findByText("New strategy")).toBeInTheDocument();
  });

  it("navigates to the selected page and closes", async () => {
    render(<CommandPalette />);
    fireEvent.click(
      screen.getByRole("button", { name: /open command palette/i }),
    );

    fireEvent.click(await screen.findByText("Shopping List"));

    expect(push).toHaveBeenCalledWith("/hestia/shopping-list");
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );
  });

  it("switches the theme from an action", async () => {
    render(<CommandPalette />);
    pressShortcut();

    fireEvent.click(await screen.findByText("Use dark theme"));

    expect(setTheme).toHaveBeenCalledWith("dark");
    expect(push).not.toHaveBeenCalled();
  });

  it("searches entities once the query is long enough", async () => {
    render(<CommandPalette />);
    pressShortcut();
    await screen.findByText("New strategy");

    fireEvent.change(screen.getByRole("combobox"), {
      target: { value: "drag" },
    });

    fireEvent.click(await screen.findByText("Dragon bolts"));
    expect(plutusApi.getAllSymbols).toHaveBeenCalledWith({
      filter: ["name:like:drag"],
      take: 5,
    });
    expect(push).toHaveBeenCalledWith("/plutus/m1/s1");
  });
});
