import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { CategoryMenu } from "./category-menu";

function openMenu() {
  fireEvent.keyDown(screen.getByRole("button", { name: "Set category for milk" }), {
    key: "Enter",
  });
}

describe("CategoryMenu", () => {
  it("marks the current category", async () => {
    render(<CategoryMenu itemName="milk" category="DairyAndEggs" onChange={vi.fn()} />);

    openMenu();

    expect(
      await screen.findByRole("menuitemradio", { name: "Dairy & Eggs" }),
    ).toHaveAttribute("aria-checked", "true");
  });

  it("reports the chosen category", async () => {
    const onChange = vi.fn();
    render(<CategoryMenu itemName="milk" category={null} onChange={onChange} />);

    openMenu();
    fireEvent.click(await screen.findByRole("menuitemradio", { name: "Dairy & Eggs" }));

    expect(onChange).toHaveBeenCalledWith("DairyAndEggs");
  });

  it("reports null when uncategorized is chosen", async () => {
    const onChange = vi.fn();
    render(<CategoryMenu itemName="milk" category="Produce" onChange={onChange} />);

    openMenu();
    fireEvent.click(await screen.findByRole("menuitemradio", { name: "Uncategorized" }));

    expect(onChange).toHaveBeenCalledWith(null);
  });
});
