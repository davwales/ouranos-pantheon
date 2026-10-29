import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { type BacktestResults } from "@/lib/api/plutus";
import { OptimizedConfigurationCard } from "./optimized-configuration-card";

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

const results: BacktestResults = {
  totalReturn: 0,
  totalReturnPercent: 0,
  maxDrawdown: 0,
  maxDrawdownPercent: 0,
  winRate: 0,
  totalTrades: 0,
  winningTrades: 0,
  losingTrades: 0,
  sharpeRatio: 0,
  sortinoRatio: null,
  calmarRatio: null,
  cagr: null,
  profitFactor: null,
  expectancy: null,
  averageTradeReturn: 0,
  bestTrade: 0,
  worstTrade: 0,
  finalBalance: 0,
  turnoverRate: 0,
  isValidated: true,
  outSampleResults: null,
  optimizedConfiguration: { mode: "Swing", inputWeights: [] },
};

describe("OptimizedConfigurationCard", () => {
  it("WhenApplyClicked_ShouldOnlyApplyAfterConfirming", () => {
    // Arrange
    const onApplyToStrategy = vi.fn();
    render(
      <OptimizedConfigurationCard
        results={results}
        onApplyToStrategy={onApplyToStrategy}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Apply to Strategy" }));
    const calledBeforeConfirm = onApplyToStrategy.mock.calls.length;
    fireEvent.click(screen.getByRole("button", { name: "Apply" }));

    // Assert
    expect(calledBeforeConfirm).toBe(0);
    expect(onApplyToStrategy).toHaveBeenCalledOnce();
  });
});
