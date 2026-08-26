import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { RecipesView } from "./recipes-view";
import type { RecipeSummary } from "@/lib/api/hestia-types";

describe("RecipesView", () => {
  it("renders recipe cards linking to detail pages when recipes are provided", () => {
    const recipes: RecipeSummary[] = [
      { id: "1", title: "Chocolate Chip Cookies", sourceUrl: "https://example.com/cookies" },
      { id: "2", title: "Spaghetti Carbonara", sourceUrl: null },
    ];

    render(<RecipesView recipes={recipes} />);

    expect(screen.getByText("Chocolate Chip Cookies")).toBeInTheDocument();
    expect(screen.getByText("Spaghetti Carbonara")).toBeInTheDocument();
    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(2);
    expect(links[0]).toHaveAttribute("href", "/hestia/recipes/1");
    expect(links[1]).toHaveAttribute("href", "/hestia/recipes/2");
  });

  it("renders empty state when no recipes", () => {
    render(<RecipesView recipes={[]} />);

    expect(
      screen.getByText("No recipes found. Add your first recipe!"),
    ).toBeInTheDocument();
  });
});
