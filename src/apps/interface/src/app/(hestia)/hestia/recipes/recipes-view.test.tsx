import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { RecipesView } from "./recipes-view";
import type { RecipeSummary } from "@/lib/api/hestia-types";

describe("RecipesView", () => {
  it("renders recipe cards when recipes are provided", () => {
    const recipes: RecipeSummary[] = [
      { id: "1", title: "Chocolate Chip Cookies", sourceUrl: "https://example.com/cookies" },
      { id: "2", title: "Spaghetti Carbonara", sourceUrl: null },
    ];

    render(<RecipesView recipes={recipes} />);

    expect(screen.getByText("Chocolate Chip Cookies")).toBeInTheDocument();
    expect(screen.getByText("Spaghetti Carbonara")).toBeInTheDocument();
    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "https://example.com/cookies");
  });

  it("renders empty state when no recipes", () => {
    render(<RecipesView recipes={[]} />);

    expect(
      screen.getByText("No recipes found. Add your first recipe!"),
    ).toBeInTheDocument();
  });
});
