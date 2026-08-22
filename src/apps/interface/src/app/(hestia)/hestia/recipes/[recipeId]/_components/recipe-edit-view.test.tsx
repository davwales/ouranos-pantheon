import { describe, it, expect, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ApiError } from "@/lib/api-client";
import { hestiaApi } from "@/lib/api/hestia";
import type { Recipe } from "@/lib/api/hestia-types";
import { RecipeEditView } from "./recipe-edit-view";

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    updateRecipe: vi.fn(),
  },
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

describe("RecipeEditView", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders the form pre-filled with the recipe data", () => {
    render(
      <RecipeEditView data={mockRecipe()} onCancel={vi.fn()} onSaved={vi.fn()} />,
    );

    expect(screen.getByLabelText(/title/i)).toHaveValue("Chocolate Cake");
    expect(screen.getByLabelText(/name for ingredient 1/i)).toHaveValue(
      "granulated sugar",
    );
    expect(screen.getByLabelText(/text for step 1/i)).toHaveValue(
      "Mix and bake for 30 minutes.",
    );
    expect(screen.getByLabelText(/notes/i)).toHaveValue("Best served warm.");
    expect(screen.getByRole("button", { name: /^save$/i })).toBeInTheDocument();
  });

  it("submits the updated recipe and calls onSaved on success", async () => {
    const onSaved = vi.fn();
    vi.mocked(hestiaApi.updateRecipe).mockResolvedValueOnce({ id: "test-recipe-1" });

    render(
      <RecipeEditView data={mockRecipe()} onCancel={vi.fn()} onSaved={onSaved} />,
    );

    fireEvent.change(screen.getByLabelText(/title/i), {
      target: { value: "Chocolate Cake (Updated)" },
    });

    fireEvent.click(screen.getByRole("button", { name: /^save$/i }));

    await waitFor(() => {
      expect(hestiaApi.updateRecipe).toHaveBeenCalledWith("test-recipe-1", {
        title: "Chocolate Cake (Updated)",
        sourceUrl: "https://example.com/cake",
        ingredients: [
          { quantity: 4, unit: "tablespoons", name: "granulated sugar" },
          { quantity: 1, unit: "tablespoon", name: "ground cinnamon" },
        ],
        steps: [{ text: "Mix and bake for 30 minutes." }],
        notes: "Best served warm.",
      });
    });

    expect(onSaved).toHaveBeenCalledTimes(1);
  });

  it("renders a generic error banner for a 400 response without calling onSaved", async () => {
    const onSaved = vi.fn();
    vi.mocked(hestiaApi.updateRecipe).mockRejectedValueOnce(
      new ApiError(400, "Required input Title was empty."),
    );

    render(
      <RecipeEditView data={mockRecipe()} onCancel={vi.fn()} onSaved={onSaved} />,
    );

    fireEvent.click(screen.getByRole("button", { name: /^save$/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Required input Title was empty.",
      );
    });

    expect(onSaved).not.toHaveBeenCalled();
  });

  it("calls onCancel when Cancel button clicked without submitting", () => {
    const onCancel = vi.fn();

    render(
      <RecipeEditView data={mockRecipe()} onCancel={onCancel} onSaved={vi.fn()} />,
    );

    fireEvent.click(screen.getByRole("button", { name: /^cancel$/i }));

    expect(onCancel).toHaveBeenCalledTimes(1);
    expect(hestiaApi.updateRecipe).not.toHaveBeenCalled();
  });
});
