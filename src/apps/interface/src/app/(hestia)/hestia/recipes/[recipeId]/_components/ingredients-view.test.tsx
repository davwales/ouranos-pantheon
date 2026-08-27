import { describe, it, expect } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import type { Ingredient } from "@/lib/api/hestia-types";
import { IngredientsView } from "./ingredients-view";

const ingredients: Ingredient[] = [
  { quantity: 4, unit: "tablespoons", name: "granulated sugar" },
  { quantity: 1, unit: "tablespoon", name: "ground cinnamon" },
  { quantity: 0, unit: "pinch", name: "salt to taste" },
];

describe("IngredientsView", () => {
  it("renders ingredients in amount, unit, and name columns at the default 1x scale", () => {
    render(<IngredientsView ingredients={ingredients} />);

    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText("tablespoons")).toBeInTheDocument();
    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("tablespoon")).toBeInTheDocument();
    expect(screen.getByText("pinch")).toBeInTheDocument();
    expect(screen.getByText("granulated sugar")).toBeInTheDocument();
    expect(screen.getByText("ground cinnamon")).toBeInTheDocument();
    expect(screen.getByText("salt to taste")).toBeInTheDocument();
    expect(screen.getByText("1x")).toBeInTheDocument();
  });

  it("scales amounts when the scale is increased and keeps units and names unchanged", () => {
    render(<IngredientsView ingredients={ingredients} />);

    fireEvent.click(screen.getByRole("button", { name: /increase scale/i }));

    expect(screen.getByText("6")).toBeInTheDocument();
    expect(screen.getByText("1½")).toBeInTheDocument();
    expect(screen.getByText("tablespoons")).toBeInTheDocument();
    expect(screen.getByText("tablespoon")).toBeInTheDocument();
    expect(screen.getByText("pinch")).toBeInTheDocument();
    expect(screen.getByText("granulated sugar")).toBeInTheDocument();
    expect(screen.getByText("ground cinnamon")).toBeInTheDocument();
    expect(screen.getByText("1.5x")).toBeInTheDocument();
  });

  it("steps back down and clamps at the minimum", () => {
    render(<IngredientsView ingredients={ingredients} />);

    const increase = screen.getByRole("button", { name: /increase scale/i });
    const decrease = screen.getByRole("button", { name: /decrease scale/i });

    fireEvent.click(increase);
    fireEvent.click(decrease);
    expect(screen.getByText("1x")).toBeInTheDocument();

    fireEvent.click(decrease);
    expect(screen.getByText("0.5x")).toBeInTheDocument();
    expect(decrease).toBeDisabled();
  });

  it("clamps at the maximum scale", () => {
    render(<IngredientsView ingredients={ingredients} />);

    const increase = screen.getByRole("button", { name: /increase scale/i });
    for (let i = 0; i < 10; i += 1) {
      fireEvent.click(increase);
    }

    expect(screen.getByText("4x")).toBeInTheDocument();
    expect(increase).toBeDisabled();
    expect(screen.getByText("16")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
  });

  it("does not render a number for zero-quantity ingredients", () => {
    render(<IngredientsView ingredients={ingredients} />);

    fireEvent.click(screen.getByRole("button", { name: /increase scale/i }));

    expect(screen.queryByText(/^0/)).not.toBeInTheDocument();
    expect(screen.getByText("pinch")).toBeInTheDocument();
    expect(screen.getByText("salt to taste")).toBeInTheDocument();
  });
});