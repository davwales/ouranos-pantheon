import { fireEvent, render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { type ChecklistItem, ShoppingChecklist } from "./shopping-checklist";

const FLOUR: ChecklistItem = {
  lineId: "recipe:flour|volume",
  label: "2 cup flour",
  name: "flour",
  category: "Pantry",
};
const MILK: ChecklistItem = {
  lineId: "manual:m1",
  label: "milk",
  name: "milk",
  category: null,
  manualItemId: "m1",
};

function renderChecklist(items: ChecklistItem[], checked: string[] = []) {
  const handlers = {
    onToggle: vi.fn(),
    onDelete: vi.fn(),
    onCategoryChange: vi.fn(),
  };
  render(<ShoppingChecklist items={items} checked={new Set(checked)} {...handlers} />);
  return handlers;
}

describe("ShoppingChecklist", () => {
  it("groups items under their category in store order, uncategorized last", () => {
    renderChecklist([MILK, FLOUR]);

    const pantry = screen.getByRole("region", { name: "Pantry" });
    const uncategorized = screen.getByRole("region", { name: "Uncategorized" });
    expect(within(pantry).getByText("2 cup flour")).toBeInTheDocument();
    expect(within(uncategorized).getByText("milk")).toBeInTheDocument();
    expect(
      pantry.compareDocumentPosition(uncategorized) & Node.DOCUMENT_POSITION_FOLLOWING,
    ).toBeTruthy();
  });

  it("renders a flat list without headings when nothing is categorized", () => {
    renderChecklist([{ ...FLOUR, category: null }, MILK]);

    expect(screen.queryByRole("heading")).not.toBeInTheDocument();
    expect(screen.getByText("2 cup flour")).toBeInTheDocument();
  });

  it("toggles by line id", () => {
    const { onToggle } = renderChecklist([MILK]);

    fireEvent.click(screen.getByRole("checkbox"));

    expect(onToggle).toHaveBeenCalledWith("manual:m1");
  });

  it("offers delete only for manual items", () => {
    const { onDelete } = renderChecklist([FLOUR, MILK]);

    expect(screen.queryByRole("button", { name: "Delete 2 cup flour" })).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Delete milk" }));

    expect(onDelete).toHaveBeenCalledWith("m1");
  });

  it("strikes through checked items", () => {
    renderChecklist([FLOUR], [FLOUR.lineId]);

    expect(screen.getByRole("listitem")).toHaveClass("line-through");
  });
});
