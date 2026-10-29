import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { type StrategyDetail } from "@/lib/api/plutus";
import { StrategyHeader } from "./strategy-header";

vi.mock("@/components/shared/responsive-dialog/responsive-dialog", () => ({
  ResponsiveDialog: ({
    children,
    open,
    trigger,
  }: {
    trigger: React.ReactNode;
    children: React.ReactNode;
    open?: boolean;
  }) => (
    <div>
      {trigger}
      {open ? <div data-testid="dialog-content">{children}</div> : null}
    </div>
  ),
}));

function renderHeader(isActive: boolean, onToggleActive: () => void) {
  const data: StrategyDetail = {
    id: "s1",
    marketId: "m1",
    name: "Momentum",
    configuration: { mode: "Swing", inputWeights: [] },
    isActive,
    createdAt: "2026-01-01T00:00:00Z",
    updatedAt: "2026-01-01T00:00:00Z",
  };
  render(
    <StrategyHeader
      data={data}
      marketId="m1"
      strategyId="s1"
      isEditing={false}
      toggling={false}
      onEdit={() => {}}
      onToggleActive={onToggleActive}
      onDelete={() => {}}
      onRunBacktest={() => {}}
      onOptimize={() => {}}
    />,
  );
}

describe("StrategyHeader", () => {
  it("WhenActive_ShouldOnlyDeactivateAfterConfirming", () => {
    // Arrange
    const onToggleActive = vi.fn();
    renderHeader(true, onToggleActive);

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Deactivate" }));
    const calledBeforeConfirm = onToggleActive.mock.calls.length;
    fireEvent.click(screen.getByRole("button", { name: "Confirm" }));

    // Assert
    expect(calledBeforeConfirm).toBe(0);
    expect(onToggleActive).toHaveBeenCalledOnce();
  });

  it("WhenInactive_ShouldActivateWithoutConfirming", () => {
    // Arrange
    const onToggleActive = vi.fn();
    renderHeader(false, onToggleActive);

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Activate" }));

    // Assert
    expect(onToggleActive).toHaveBeenCalledOnce();
    expect(screen.queryByTestId("dialog-content")).not.toBeInTheDocument();
  });
});
