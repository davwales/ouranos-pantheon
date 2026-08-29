import { describe, it, expect, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { AddToShoppingListButton } from "./add-to-shopping-list-button";

describe("AddToShoppingListButton", () => {
  it("renders add label when not in list", () => {
    render(
      <AddToShoppingListButton
        recipeId="r1"
        isInList={false}
        onToggle={vi.fn()}
      />,
    );

    expect(
      screen.getByRole("button", { name: /add to shopping list/i }),
    ).toBeInTheDocument();
  });

  it("renders in-list label when in list", () => {
    render(
      <AddToShoppingListButton
        recipeId="r1"
        isInList={true}
        onToggle={vi.fn()}
      />,
    );

    expect(
      screen.getByRole("button", { name: /in shopping list/i }),
    ).toBeInTheDocument();
  });

  it("calls onToggle with recipeId when clicked", () => {
    const onToggle = vi.fn();
    render(
      <AddToShoppingListButton
        recipeId="r1"
        isInList={false}
        onToggle={onToggle}
      />,
    );

    fireEvent.click(screen.getByRole("button"));

    expect(onToggle).toHaveBeenCalledWith("r1");
  });

  it("does not call onToggle when disabled", () => {
    const onToggle = vi.fn();
    render(
      <AddToShoppingListButton
        recipeId="r1"
        isInList={false}
        onToggle={onToggle}
        disabled
      />,
    );

    fireEvent.click(screen.getByRole("button"));

    expect(onToggle).not.toHaveBeenCalled();
  });
});
