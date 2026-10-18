import { MobileContextMenu } from "@/components/shared/responsive-context-menu/mobile-context-menu";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

function renderMenu({ disabled, onChildClick }: { disabled: boolean; onChildClick?: () => void }) {
  return render(
    <MobileContextMenu
      actions={[{ label: "Edit", icon: null, onClick: vi.fn() }]}
      title="Actions"
      description="Perform an action on the message."
      disabled={disabled}
    >
      <span onClick={onChildClick}>Expand</span>
    </MobileContextMenu>,
  );
}

describe("MobileContextMenu", () => {
  it("WhenDisabled_ShouldStillDeliverClicksToChildren", () => {
    // Arrange
    const onChildClick = vi.fn();
    renderMenu({ disabled: true, onChildClick });

    // Act
    fireEvent.click(screen.getByText("Expand"));

    // Assert
    expect(onChildClick).toHaveBeenCalledOnce();
    expect(screen.getByText("Expand").closest("button:disabled")).toBeNull();
  });

  it("WhenDisabled_ShouldNotOpenTheDrawer", () => {
    // Arrange
    renderMenu({ disabled: true });
    const trigger = screen.getByRole("button");

    // Act
    fireEvent.click(trigger);

    // Assert
    expect(trigger).toHaveAttribute("aria-disabled", "true");
    expect(screen.queryByText("Actions")).not.toBeInTheDocument();
  });

  it("WhenEnabled_ShouldOpenTheDrawer", () => {
    // Arrange
    renderMenu({ disabled: false });

    // Act
    fireEvent.click(screen.getByRole("button"));

    // Assert
    expect(screen.getByText("Actions")).toBeInTheDocument();
  });
});
