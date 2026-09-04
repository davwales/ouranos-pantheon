import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { hestiaApi } from "@/lib/api/hestia";
import type { ShoppingListResponse } from "@/lib/api/hestia-types";
import { ShoppingListView } from "./shopping-list-view";

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    updateCheckedItems: vi.fn(),
    addManualItem: vi.fn(),
    deleteManualItem: vi.fn(),
    toggleRecipeInShoppingList: vi.fn(),
  },
}));

vi.mock("@/components/shared/responsive-dialog/responsive-dialog", () => ({
  ResponsiveDialog: ({
    children,
    open,
    trigger,
  }: {
    title: string;
    description: string;
    trigger: React.ReactNode;
    children: React.ReactNode;
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
  }) => (
    <div data-testid="responsive-dialog" data-open={open ? "true" : "false"}>
      {trigger}
      {open ? <div data-testid="dialog-content">{children}</div> : null}
    </div>
  ),
}));

function mockShoppingList(
  overrides: Partial<ShoppingListResponse> = {},
): ShoppingListResponse {
  return {
    recipeIds: ["r1"],
    recipes: [{ id: "r1", title: "Pancakes" }],
    consolidatedIngredients: [
      { id: "line-1", name: "flour", unit: "cups", quantity: 2 },
    ],
    manualItems: [{ id: "m1", text: "milk" }],
    checkedItemIds: [],
    ...overrides,
  };
}

describe("ShoppingListView", () => {
  beforeEach(() => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("renders ingredients and manual items", () => {
    render(<ShoppingListView data={mockShoppingList()} onReload={vi.fn()} />);

    expect(screen.getByText("2 cups flour")).toBeInTheDocument();
    expect(screen.getByText("milk")).toBeInTheDocument();
  });

  it("calls updateCheckedItems after debounce when a checkbox is toggled", async () => {
    vi.mocked(hestiaApi.updateCheckedItems).mockResolvedValue({
      checkedItemIds: ["line-1"],
    });

    render(<ShoppingListView data={mockShoppingList()} onReload={vi.fn()} />);

    fireEvent.click(screen.getAllByRole("checkbox")[0]);

    expect(hestiaApi.updateCheckedItems).not.toHaveBeenCalled();

    vi.advanceTimersByTime(400);

    await waitFor(() => {
      expect(hestiaApi.updateCheckedItems).toHaveBeenCalledWith({
        checkedItemIds: ["line-1"],
      });
    });
  });

  it("calls addManualItem and reloads when adding an item", async () => {
    const onReload = vi.fn();
    vi.mocked(hestiaApi.addManualItem).mockResolvedValue({ id: "m2", text: "eggs" });

    render(<ShoppingListView data={mockShoppingList()} onReload={onReload} />);

    const input = screen.getByPlaceholderText(/add an item/i);
    fireEvent.change(input, { target: { value: "eggs" } });
    fireEvent.click(screen.getByRole("button", { name: /add/i }));

    await waitFor(() => {
      expect(hestiaApi.addManualItem).toHaveBeenCalledWith({ text: "eggs" });
    });

    expect(onReload).toHaveBeenCalled();
  });

  it("removes a recipe via the toggle endpoint and reloads", async () => {
    const onReload = vi.fn();
    vi.mocked(hestiaApi.toggleRecipeInShoppingList).mockResolvedValue({
      recipeId: "r1",
      isInList: false,
    });

    render(<ShoppingListView data={mockShoppingList()} onReload={onReload} />);

    fireEvent.click(screen.getByRole("button", { name: /remove pancakes/i }));

    await waitFor(() => {
      expect(hestiaApi.toggleRecipeInShoppingList).toHaveBeenCalledWith("r1");
    });

    expect(onReload).toHaveBeenCalled();
  });

  it("clears all recipes with sequential toggles and reloads", async () => {
    const onReload = vi.fn();
    vi.mocked(hestiaApi.toggleRecipeInShoppingList).mockResolvedValue({
      recipeId: "r1",
      isInList: false,
    });

    render(
      <ShoppingListView
        data={mockShoppingList({
          recipes: [
            { id: "r1", title: "Pancakes" },
            { id: "r2", title: "Soup" },
          ],
        })}
        onReload={onReload}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /clear all recipes/i }));
    fireEvent.click(await screen.findByRole("button", { name: /^confirm$/i }));

    await waitFor(() => {
      expect(hestiaApi.toggleRecipeInShoppingList).toHaveBeenNthCalledWith(
        1,
        "r1",
      );
      expect(hestiaApi.toggleRecipeInShoppingList).toHaveBeenNthCalledWith(
        2,
        "r2",
      );
    });

    expect(onReload).toHaveBeenCalled();
  });

  it("calls deleteManualItem and reloads when deleting a manual item", async () => {
    const onReload = vi.fn();
    vi.mocked(hestiaApi.deleteManualItem).mockResolvedValue({ id: "m1" });

    render(<ShoppingListView data={mockShoppingList()} onReload={onReload} />);

    fireEvent.click(screen.getByRole("button", { name: /delete milk/i }));

    await waitFor(() => {
      expect(hestiaApi.deleteManualItem).toHaveBeenCalledWith("m1");
    });

    expect(onReload).toHaveBeenCalled();
  });

  it("calls addManualItem and reloads, keeping the input text on failure", async () => {
    const onReload = vi.fn();
    vi.mocked(hestiaApi.addManualItem).mockRejectedValue(
      new Error("Failed to add item"),
    );

    render(<ShoppingListView data={mockShoppingList()} onReload={onReload} />);

    const input = screen.getByPlaceholderText(/add an item/i);
    fireEvent.change(input, { target: { value: "eggs" } });
    fireEvent.click(screen.getByRole("button", { name: /add/i }));

    await waitFor(() => {
      expect(screen.getByText(/failed to add item/i)).toBeInTheDocument();
    });

    expect(onReload).not.toHaveBeenCalled();
    expect(input).toHaveValue("eggs");
  });

  it("renders empty state when there are no items", () => {
    render(
      <ShoppingListView
        data={mockShoppingList({
          recipeIds: [],
          recipes: [],
          consolidatedIngredients: [],
          manualItems: [],
        })}
        onReload={vi.fn()}
      />,
    );

    expect(
      screen.getByText(/your shopping list is empty/i),
    ).toBeInTheDocument();
  });

  it("hides the recipes card when no recipes are in the list", () => {
    render(
      <ShoppingListView
        data={mockShoppingList({ recipeIds: [], recipes: [] })}
        onReload={vi.fn()}
      />,
    );

    expect(screen.queryByText("Pancakes")).not.toBeInTheDocument();
    expect(screen.getByText("2 cups flour")).toBeInTheDocument();
  });
});
