import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { DataTableEmptyState, DataTableRefreshError } from "./data-table-status";

describe("DataTableEmptyState", () => {
  it("shows no results when the fetch succeeded", () => {
    // Act
    render(<DataTableEmptyState />);

    // Assert
    expect(screen.getByText("No results.")).toBeTruthy();
  });

  it("shows the error with a retry instead of no results", () => {
    // Arrange
    const onRetry = vi.fn();
    render(<DataTableEmptyState error={new Error("Gateway timeout")} onRetry={onRetry} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Retry" }));

    // Assert
    expect(screen.queryByText("No results.")).toBeNull();
    expect(screen.getByText(/Gateway timeout/)).toBeTruthy();
    expect(onRetry).toHaveBeenCalledOnce();
  });
});

describe("DataTableRefreshError", () => {
  it("warns above stale rows when the refresh failed", () => {
    // Act
    render(<DataTableRefreshError error={new Error("Gateway timeout")} hasRows />);

    // Assert
    expect(screen.getByRole("alert").textContent).toContain("Couldn’t refresh");
  });

  it("renders nothing without rows or without an error", () => {
    // Act
    const { container } = render(
      <>
        <DataTableRefreshError error={new Error("Gateway timeout")} hasRows={false} />
        <DataTableRefreshError hasRows />
      </>,
    );

    // Assert
    expect(container.innerHTML).toBe("");
  });
});
