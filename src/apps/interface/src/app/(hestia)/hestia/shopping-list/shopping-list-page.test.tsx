import { describe, it, expect, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { hestiaApi } from "@/lib/api/hestia";
import type { ShoppingListResponse } from "@/lib/api/hestia-types";
import ShoppingListPage from "./page";

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    getShoppingList: vi.fn(),
  },
}));

const emptyShoppingList: ShoppingListResponse = {
  recipeIds: [],
  recipes: [],
  consolidatedIngredients: [],
  manualItems: [],
  checkedItemIds: [],
};

describe("ShoppingListPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders the shopping list returned by the api", async () => {
    vi.mocked(hestiaApi.getShoppingList).mockResolvedValue({
      ...emptyShoppingList,
      manualItems: [{ id: "m1", text: "milk" }],
    });

    render(<ShoppingListPage />);

    expect(await screen.findByText("milk")).toBeInTheDocument();
    expect(hestiaApi.getShoppingList).toHaveBeenCalledTimes(1);
  });

  it("renders the error banner and retries the request", async () => {
    vi.mocked(hestiaApi.getShoppingList).mockRejectedValueOnce(
      new Error("Gateway down"),
    );
    vi.mocked(hestiaApi.getShoppingList).mockResolvedValueOnce(
      emptyShoppingList,
    );

    render(<ShoppingListPage />);

    expect(await screen.findByText(/gateway down/i)).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /retry/i }));

    await waitFor(() => {
      expect(hestiaApi.getShoppingList).toHaveBeenCalledTimes(2);
    });
    expect(await screen.findByText(/your shopping list is empty/i)).toBeInTheDocument();
  });
});