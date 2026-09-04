import { describe, it, expect, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { RecipeCard } from "./recipe-card";
import type { RecipeSummary } from "@/lib/api/hestia-types";

function buildRecipe(overrides: Partial<RecipeSummary> = {}): RecipeSummary {
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

describe("RecipeCard", () => {
  it("renders the title as a stretched link to the detail page", () => {
    render(
      <RecipeCard recipe={buildRecipe()} isInList={false} onToggle={vi.fn()} />,
    );

    const link = screen.getByRole("link", { name: "Chocolate Chip Cookies" });
    expect(link).toHaveAttribute("href", "/hestia/recipes/r1");
    expect(link.className).toContain("after:absolute");
  });

  it("renders an imported badge with the source hostname for imported recipes", () => {
    render(
      <RecipeCard recipe={buildRecipe()} isInList={false} onToggle={vi.fn()} />,
    );

    expect(screen.getByText("Imported · example.com")).toBeInTheDocument();
  });

  it("renders the hostname on the failed badge so the source is identifiable", () => {
    render(
      <RecipeCard
        recipe={buildRecipe({ importStatus: "Failed" })}
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    expect(
      screen.getByText("Import failed · example.com"),
    ).toBeInTheDocument();
  });

  it.each([
    { importStatus: "Failed" as const, sourceUrl: null },
    { importStatus: "Imported" as const, sourceUrl: "not-a-url" },
  ])("renders the $importStatus badge without a hostname when the source url is missing or malformed", ({ importStatus, sourceUrl }) => {
    render(
      <RecipeCard
        recipe={buildRecipe({ importStatus, sourceUrl })}
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    if (importStatus === "Failed") {
      expect(screen.getByText("Import failed")).toBeInTheDocument();
    } else {
      expect(screen.getByText("Imported")).toBeInTheDocument();
    }
  });

  it("renders no import badge for manual recipes", () => {
    render(
      <RecipeCard
        recipe={buildRecipe({ importStatus: "None" })}
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.queryByText("Imported")).not.toBeInTheDocument();
    expect(screen.queryByText("Importing…")).not.toBeInTheDocument();
    expect(screen.queryByText("Import failed")).not.toBeInTheDocument();
  });

  it("renders the ingredient and step counts with pluralization", () => {
    render(
      <RecipeCard recipe={buildRecipe()} isInList={false} onToggle={vi.fn()} />,
    );

    expect(screen.getByText("12 ingredients · 8 steps")).toBeInTheDocument();
  });

  it("renders singular counts", () => {
    render(
      <RecipeCard
        recipe={buildRecipe({ ingredientCount: 1, stepCount: 1 })}
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("1 ingredient · 1 step")).toBeInTheDocument();
  });

  it("renders the added date in a consistent format", () => {
    render(
      <RecipeCard recipe={buildRecipe()} isInList={false} onToggle={vi.fn()} />,
    );

    const expected = new Intl.DateTimeFormat(undefined, {
      month: "short",
      day: "numeric",
      year: "numeric",
    }).format(new Date("2025-03-12T10:00:00Z"));

    expect(screen.getByText(`Added ${expected}`)).toBeInTheDocument();
  });

  it("renders an importing badge instead of counts while importing", () => {
    render(
      <RecipeCard
        recipe={buildRecipe({ importStatus: "Importing", ingredientCount: 0, stepCount: 0 })}
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("Importing…")).toBeInTheDocument();
    expect(screen.queryByText(/ingredients/)).not.toBeInTheDocument();
  });

  it("renders a failed import badge", () => {
    render(
      <RecipeCard
        recipe={buildRecipe({ importStatus: "Failed", sourceUrl: null })}
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText(/Import failed/)).toBeInTheDocument();
  });

  it("disables the shopping list button while a recipe is importing", () => {
    render(
      <RecipeCard
        recipe={buildRecipe({ importStatus: "Importing" })}
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByRole("button")).toBeDisabled();
  });

  it("disables the shopping list button when an import has failed", () => {
    const onToggle = vi.fn();
    render(
      <RecipeCard
        recipe={buildRecipe({ importStatus: "Failed" })}
        isInList={false}
        onToggle={onToggle}
      />,
    );

    const button = screen.getByRole("button");
    expect(button).toBeDisabled();

    fireEvent.click(button);

    expect(onToggle).not.toHaveBeenCalled();
  });

  it("renders the shopping list button in the footer and toggles without navigating", () => {
    const onToggle = vi.fn();
    render(
      <RecipeCard recipe={buildRecipe()} isInList={true} onToggle={onToggle} />,
    );

    expect(
      screen.getByRole("button", { name: /in shopping list/i }),
    ).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button"));

    expect(onToggle).toHaveBeenCalledWith("r1");
  });
});