import { MessageActionBar } from "@/app/(hermes)/hermes/components/message-action-bar";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

describe("MessageActionBar", () => {
  it("ShouldRenderALabelledButtonPerAction", () => {
    // Arrange
    const onCopy = vi.fn();
    render(
      <MessageActionBar
        actions={[
          { label: "Copy", icon: null, onClick: onCopy },
          { label: "Edit", icon: null, onClick: vi.fn() },
        ]}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Copy" }));

    // Assert
    expect(screen.getByRole("toolbar", { name: "Message actions" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Edit" })).toBeInTheDocument();
    expect(onCopy).toHaveBeenCalledOnce();
  });

  it("WhenDisabled_ShouldNotRunActions", () => {
    // Arrange
    const onEdit = vi.fn();
    render(<MessageActionBar actions={[{ label: "Edit", icon: null, onClick: onEdit }]} disabled />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Edit" }));

    // Assert
    expect(screen.getByRole("button", { name: "Edit" })).toBeDisabled();
    expect(onEdit).not.toHaveBeenCalled();
  });

  it("WhenPinned_ShouldStayVisibleWithoutHover", () => {
    // Arrange & Act
    render(<MessageActionBar actions={[]} isPinned />);

    // Assert
    expect(screen.getByRole("toolbar")).toHaveClass("opacity-100");
    expect(screen.getByRole("toolbar")).not.toHaveClass("opacity-0");
  });

  it("WhenNotPinned_ShouldRevealOnHoverFocusOrTouch", () => {
    // Arrange & Act
    render(<MessageActionBar actions={[]} />);

    // Assert
    expect(screen.getByRole("toolbar")).toHaveClass(
      "opacity-0",
      "group-hover/message:opacity-100",
      "group-focus-within/message:opacity-100",
      "[@media(hover:none)]:opacity-100",
    );
  });
});
