import { describe, it, expect, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { IngredientChecklist } from "./ingredient-checklist";
import type { ConsolidatedIngredient } from "@/lib/api/hestia-types";

function mockIngredient(overrides: Partial<ConsolidatedIngredient> = {}): ConsolidatedIngredient {
  return {
    id: "line-1",
    name: "flour",
    unit: "cups",
    quantity: 2,
    ...overrides,
  };
}

describe("IngredientChecklist", () => {
  it("renders quantity, unit, and name", () => {
    render(
      <IngredientChecklist
        ingredients={[mockIngredient()]}
        checked={new Set()}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("2 cups flour")).toBeInTheDocument();
  });

  it("hides zero quantity", () => {
    render(
      <IngredientChecklist
        ingredients={[mockIngredient({ quantity: 0 })]}
        checked={new Set()}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("cups flour")).toBeInTheDocument();
  });

  it("omits empty unit without extra whitespace", () => {
    render(
      <IngredientChecklist
        ingredients={[mockIngredient({ quantity: 2, unit: "", name: "eggs" })]}
        checked={new Set()}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("2 eggs")).toBeInTheDocument();
  });

  it("calls onToggle when checkbox is changed", () => {
    const onToggle = vi.fn();
    render(
      <IngredientChecklist
        ingredients={[mockIngredient({ id: "line-1" })]}
        checked={new Set()}
        onToggle={onToggle}
      />,
    );

    fireEvent.click(screen.getByRole("checkbox"));

    expect(onToggle).toHaveBeenCalledWith("line-1");
  });

  it("applies strikethrough styling when checked", () => {
    render(
      <IngredientChecklist
        ingredients={[mockIngredient({ id: "line-1" })]}
        checked={new Set(["line-1"])}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("2 cups flour").parentElement).toHaveClass("line-through");
  });
});
