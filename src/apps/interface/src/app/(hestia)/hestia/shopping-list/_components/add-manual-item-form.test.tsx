import { describe, it, expect, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { AddManualItemForm } from "./add-manual-item-form";

describe("AddManualItemForm", () => {
  it("disables Add when input is empty", () => {
    render(<AddManualItemForm onAdd={vi.fn()} adding={false} />);

    expect(screen.getByRole("button", { name: /add/i })).toBeDisabled();
  });

  it("disables Add while adding", () => {
    render(<AddManualItemForm onAdd={vi.fn().mockResolvedValue(true)} adding={true} />);

    const input = screen.getByPlaceholderText(/add an item/i);
    fireEvent.change(input, { target: { value: "milk" } });

    expect(screen.getByRole("button", { name: /add/i })).toBeDisabled();
  });

  it("submits trimmed text and clears input on success", async () => {
    const onAdd = vi.fn().mockResolvedValue(true);
    render(<AddManualItemForm onAdd={onAdd} adding={false} />);

    const input = screen.getByPlaceholderText(/add an item/i);
    fireEvent.change(input, { target: { value: "  milk  " } });
    fireEvent.click(screen.getByRole("button", { name: /add/i }));

    await waitFor(() => {
      expect(onAdd).toHaveBeenCalledWith("milk");
    });
    expect(input).toHaveValue("");
  });

  it("keeps input text when the add fails", async () => {
    const onAdd = vi.fn().mockResolvedValue(false);
    render(<AddManualItemForm onAdd={onAdd} adding={false} />);

    const input = screen.getByPlaceholderText(/add an item/i);
    fireEvent.change(input, { target: { value: "milk" } });
    fireEvent.click(screen.getByRole("button", { name: /add/i }));

    await waitFor(() => {
      expect(onAdd).toHaveBeenCalledWith("milk");
    });
    expect(input).toHaveValue("milk");
  });
});
