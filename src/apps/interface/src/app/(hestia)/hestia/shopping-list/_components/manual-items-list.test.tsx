import { describe, it, expect, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { ManualItemsList } from "./manual-items-list";
import type { ManualItem } from "@/lib/api/hestia-types";

function mockItem(overrides: Partial<ManualItem> = {}): ManualItem {
  return {
    id: "m1",
    text: "milk",
    ...overrides,
  };
}

describe("ManualItemsList", () => {
  it("renders item text", () => {
    render(
      <ManualItemsList
        items={[mockItem()]}
        checked={new Set()}
        onToggle={vi.fn()}
        onDelete={vi.fn()}
      />,
    );

    expect(screen.getByText("milk")).toBeInTheDocument();
  });

  it("calls onToggle with the manual line id", () => {
    const onToggle = vi.fn();
    render(
      <ManualItemsList
        items={[mockItem()]}
        checked={new Set()}
        onToggle={onToggle}
        onDelete={vi.fn()}
      />,
    );

    fireEvent.click(screen.getByRole("checkbox"));

    expect(onToggle).toHaveBeenCalledWith("manual:m1");
  });

  it("calls onDelete via the delete button", () => {
    const onDelete = vi.fn();
    render(
      <ManualItemsList
        items={[mockItem()]}
        checked={new Set()}
        onToggle={vi.fn()}
        onDelete={onDelete}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /delete milk/i }));

    expect(onDelete).toHaveBeenCalledWith("m1");
  });

  it("applies strikethrough styling when checked", () => {
    render(
      <ManualItemsList
        items={[mockItem()]}
        checked={new Set(["manual:m1"])}
        onToggle={vi.fn()}
        onDelete={vi.fn()}
      />,
    );

    expect(screen.getByText("milk").closest("div")).toHaveClass("line-through");
  });
});