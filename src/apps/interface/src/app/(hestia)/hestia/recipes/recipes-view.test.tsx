import { describe, it, expect, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { RecipesView } from "./recipes-view";
import type { RecipeSummary } from "@/lib/api/hestia-types";

function buildRecipe(overrides: Partial<RecipeSummary> = {}): RecipeSummary {
  return {
    id: "r1",
    title: "Chocolate Chip Cookies",
    sourceUrl: null,
    createdAt: "2025-03-12T10:00:00Z",
    ingredientCount: 12,
    stepCount: 8,
    importStatus: "Imported",
    ...overrides,
  };
}

describe("RecipesView", () => {
  it("renders recipe cards linking to detail pages when recipes are provided", () => {
    const recipes: RecipeSummary[] = [
      buildRecipe({ id: "1", title: "Chocolate Chip Cookies" }),
      buildRecipe({ id: "2", title: "Spaghetti Carbonara" }),
    ];

    render(
      <RecipesView
        recipes={recipes}
        inListRecipeIds={new Set()}
        onToggleRecipe={vi.fn()}
      />,
    );

    expect(screen.getByText("Chocolate Chip Cookies")).toBeInTheDocument();
    expect(screen.getByText("Spaghetti Carbonara")).toBeInTheDocument();
    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(2);
    expect(links[0]).toHaveAttribute("href", "/hestia/recipes/1");
    expect(links[1]).toHaveAttribute("href", "/hestia/recipes/2");
  });

  it("renders empty state when no recipes", () => {
    render(
      <RecipesView
        recipes={[]}
        inListRecipeIds={new Set()}
        onToggleRecipe={vi.fn()}
      />,
    );

    expect(
      screen.getByText("No recipes found. Add your first recipe!"),
    ).toBeInTheDocument();
  });

  it("shows toggle buttons and calls onToggleRecipe without navigating", () => {
    const onToggleRecipe = vi.fn();
    const recipes: RecipeSummary[] = [
      buildRecipe({ id: "1", title: "Chocolate Chip Cookies" }),
    ];

    render(
      <RecipesView
        recipes={recipes}
        inListRecipeIds={new Set(["1"])}
        onToggleRecipe={onToggleRecipe}
      />,
    );

    expect(
      screen.getByRole("button", { name: /in shopping list/i }),
    ).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button"));

    expect(onToggleRecipe).toHaveBeenCalledWith("1");
  });
});