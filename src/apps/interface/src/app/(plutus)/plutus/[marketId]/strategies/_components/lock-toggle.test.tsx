import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { LockToggle } from "./lock-toggle";

describe("LockToggle", () => {
  it("WhenLocked_ShouldBePressedAndFilled", () => {
    // Arrange & Act
    render(<LockToggle label="Max Positions" locked onToggle={() => {}} />);

    // Assert
    const button = screen.getByRole("button", { name: "Lock Max Positions" });
    expect(button).toHaveAttribute("aria-pressed", "true");
    expect(button.firstElementChild).toHaveClass("bg-primary");
  });

  it("WhenUnlocked_ShouldBeUnfilledAndToggleOnClick", () => {
    // Arrange
    const onToggle = vi.fn();
    render(<LockToggle label="Max Positions" locked={false} onToggle={onToggle} />);
    const button = screen.getByRole("button", { name: "Lock Max Positions" });

    // Act
    fireEvent.click(button);

    // Assert
    expect(button).toHaveAttribute("aria-pressed", "false");
    expect(button.firstElementChild).not.toHaveClass("bg-primary");
    expect(onToggle).toHaveBeenCalledOnce();
  });

  it("WhenNeverOptimized_ShouldShowAFixedIconWithoutAButton", () => {
    // Arrange & Act
    render(<LockToggle label="Min Price" locked={false} />);

    // Assert
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
    expect(
      screen.getByRole("img", { name: /never optimized/ }),
    ).toBeInTheDocument();
  });
});
