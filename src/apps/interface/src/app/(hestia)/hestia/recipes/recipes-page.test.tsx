import { describe, it, expect, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ApiError } from "@/lib/api-client";
import { hestiaApi } from "@/lib/api/hestia";
import type { RecipeSummary } from "@/lib/api/hestia-types";
import RecipesPage from "./page";

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    getAllRecipes: vi.fn(),
    getShoppingList: vi.fn().mockResolvedValue({
      recipeIds: [],
      consolidatedIngredients: [],
      manualItems: [],
      checkedItemIds: [],
    }),
    toggleRecipeInShoppingList: vi.fn(),
  },
}));

vi.mock("./_components/import-recipe-dialog", () => ({
  ImportRecipeDialog: () => null,
}));

function buildSummary(overrides: Partial<RecipeSummary> = {}): RecipeSummary {
  return {
    id: "r1",
    title: "Chocolate Chip Cookies",
    sourceUrl: "https://www.example.com/cookies",
    createdAt: "2025-03-12T10:00:00Z",
    ingredientCount: 12,
    stepCount: 8,
    importStatus: "Imported",
    ...overrides,
  };
}

function mockPagedRecipes(items: RecipeSummary[], totalCount = items.length) {
  return vi.mocked(hestiaApi.getAllRecipes).mockResolvedValueOnce({
    items,
    totalCount,
    skip: 0,
    take: 50,
  });
}

const emptyShoppingList = {
  recipeIds: [],
  consolidatedIngredients: [],
  manualItems: [],
  checkedItemIds: [],
};

describe("RecipesPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(hestiaApi.getShoppingList).mockResolvedValue(emptyShoppingList);
  });

  it("renders the recipes returned by the api", async () => {
    mockPagedRecipes([
      buildSummary({ id: "r1", title: "Chocolate Chip Cookies" }),
      buildSummary({ id: "r2", title: "Spaghetti Carbonara" }),
    ]);

    render(<RecipesPage />);

    expect(
      await screen.findByText("Chocolate Chip Cookies"),
    ).toBeInTheDocument();
    expect(screen.getByText("Spaghetti Carbonara")).toBeInTheDocument();
    expect(screen.queryByText(/Showing /)).not.toBeInTheDocument();
  });

  it("renders the pagination footer when more recipes exist than are shown", async () => {
    mockPagedRecipes(
      [buildSummary()],
      55,
    );

    render(<RecipesPage />);

    expect(await screen.findByText("Showing 1 of 55 recipes")).toBeInTheDocument();
  });

  it("shows the in-list state and refreshes the shopping list after toggling", async () => {
    vi.mocked(hestiaApi.getShoppingList).mockResolvedValue({
      ...emptyShoppingList,
      recipeIds: ["r1"],
    });
    mockPagedRecipes([
      buildSummary({ id: "r1", title: "Chocolate Chip Cookies" }),
      buildSummary({ id: "r2", title: "Spaghetti Carbonara" }),
    ]);
    vi.mocked(hestiaApi.toggleRecipeInShoppingList).mockResolvedValueOnce({
      recipeId: "r2",
      isInList: true,
    });

    render(<RecipesPage />);

    expect(
      await screen.findByRole("button", { name: /in shopping list/i }),
    ).toBeInTheDocument();

    fireEvent.click(
      screen.getByRole("button", { name: /add to shopping list/i }),
    );

    await waitFor(() => {
      expect(hestiaApi.toggleRecipeInShoppingList).toHaveBeenCalledWith("r2");
      expect(hestiaApi.getShoppingList).toHaveBeenCalledTimes(2);
    });
  });

  it("renders an error banner when toggling the shopping list membership fails", async () => {
    mockPagedRecipes([buildSummary()]);
    vi.mocked(hestiaApi.toggleRecipeInShoppingList).mockRejectedValueOnce(
      new ApiError(500, "Failed to update list"),
    );

    render(<RecipesPage />);

    const button = await screen.findByRole("button", {
      name: /add to shopping list/i,
    });
    fireEvent.click(button);

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Failed to update list",
      );
    });
  });

  it("renders an error alert when loading recipes fails", async () => {
    vi.mocked(hestiaApi.getAllRecipes).mockRejectedValueOnce(
      new ApiError(500, "Database unavailable"),
    );

    render(<RecipesPage />);

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Failed to load recipes: Database unavailable",
    );
  });
});