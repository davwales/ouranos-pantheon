import { describe, it, expect, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ApiError } from "@/lib/api-client";
import { hestiaApi } from "@/lib/api/hestia";
import type { Recipe } from "@/lib/api/hestia-types";
import RecipeDetailPage from "./page";

vi.mock("next/navigation", () => ({
  useParams: () => ({ recipeId: "test-recipe-1" }),
}));

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    getRecipe: vi.fn(),
    updateRecipe: vi.fn(),
    getRecipeHistory: vi.fn().mockResolvedValue({
      recipeId: "test-recipe-1",
      events: [],
    }),
    revertRecipe: vi.fn(),
  },
}));

vi.mock("@/components/shared/responsive-dialog/responsive-dialog", () => ({
  ResponsiveDialog: ({
    children,
    open,
  }: {
    title: string;
    description: string;
    trigger: React.ReactNode;
    children: React.ReactNode;
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
  }) => (
    <div data-testid="responsive-dialog" data-open={open ? "true" : "false"}>
      {open ? <div data-testid="dialog-content">{children}</div> : null}
    </div>
  ),
}));

function mockRecipe(overrides: Partial<Recipe> = {}): Recipe {
  return {
    id: "test-recipe-1",
    title: "Chocolate Cake",
    sourceUrl: "https://example.com/cake",
    steps: [{ text: "Mix and bake for 30 minutes." }],
    ingredients: [
      { quantity: 4, unit: "tablespoons", name: "granulated sugar" },
      { quantity: 1, unit: "tablespoon", name: "ground cinnamon" },
    ],
    notes: "Best served warm.",
    createdAt: "2025-01-01T00:00:00.000Z",
    ...overrides,
  };
}

describe("RecipeDetailPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders recipe title, ingredients, instructions, and notes when loaded", async () => {
    vi.mocked(hestiaApi.getRecipe).mockResolvedValueOnce(mockRecipe());

    render(<RecipeDetailPage />);

    expect(await screen.findByText("Chocolate Cake")).toBeInTheDocument();
    expect(screen.getByText("granulated sugar")).toBeInTheDocument();
    expect(
      screen.getByText("Mix and bake for 30 minutes."),
    ).toBeInTheDocument();
    expect(screen.getByText("Best served warm.")).toBeInTheDocument();
  });

  it("renders NotFoundCard when API errors", async () => {
    vi.mocked(hestiaApi.getRecipe).mockRejectedValueOnce(
      new ApiError(404, "Not found"),
    );

    render(<RecipeDetailPage />);

    expect(await screen.findByText("Recipe not found")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /back to recipes/i }),
    ).toBeInTheDocument();
  });

  it("toggles edit mode and shows the pre-filled edit form when Edit button clicked", async () => {
    vi.mocked(hestiaApi.getRecipe).mockResolvedValueOnce(mockRecipe());

    render(<RecipeDetailPage />);

    await screen.findByText("Chocolate Cake");

    fireEvent.click(screen.getByRole("button", { name: /^edit$/i }));

    await waitFor(() => {
      expect(screen.getByLabelText(/title/i)).toHaveValue("Chocolate Cake");
    });

    expect(
      screen.getByLabelText(/name for ingredient 1/i),
    ).toHaveValue("granulated sugar");
    expect(screen.getByRole("button", { name: /^save$/i })).toBeInTheDocument();
  });

  it("does not fetch version history until the dialog is opened", async () => {
    vi.mocked(hestiaApi.getRecipe).mockResolvedValueOnce(mockRecipe());

    render(<RecipeDetailPage />);

    await screen.findByText("Chocolate Cake");

    expect(hestiaApi.getRecipeHistory).not.toHaveBeenCalled();

    fireEvent.click(screen.getByRole("button", { name: /version history/i }));

    await waitFor(() => {
      expect(hestiaApi.getRecipeHistory).toHaveBeenCalledTimes(1);
    });
  });

  it("opens Version History dialog when button clicked", async () => {
    vi.mocked(hestiaApi.getRecipe).mockResolvedValueOnce(mockRecipe());

    render(<RecipeDetailPage />);

    await screen.findByText("Chocolate Cake");

    fireEvent.click(screen.getByRole("button", { name: /version history/i }));

    await waitFor(() => {
      expect(screen.getByTestId("dialog-content")).toBeInTheDocument();
      expect(
        screen.getByText(/no version history yet/i),
      ).toBeInTheDocument();
    });
  });
});