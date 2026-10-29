import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api-client";
import { NotFoundCard } from "./not-found-card";

describe("NotFoundCard", () => {
  it("shows not found without an error", () => {
    // Act
    render(<NotFoundCard title="Group not found" backHref="/groups" />);

    // Assert
    expect(screen.getByText("Group not found")).toBeTruthy();
  });

  it("shows not found for a 404", () => {
    // Act
    render(
      <NotFoundCard
        title="Group not found"
        backHref="/groups"
        error={new ApiError(404, "Not Found")}
        onRetry={vi.fn()}
      />,
    );

    // Assert
    expect(screen.getByText("Group not found")).toBeTruthy();
    expect(screen.queryByRole("button", { name: "Retry" })).toBeNull();
  });

  it("shows a retryable error for any other failure", () => {
    // Arrange
    const onRetry = vi.fn();
    render(
      <NotFoundCard
        title="Group not found"
        backHref="/groups"
        error={new ApiError(500, "Server error")}
        onRetry={onRetry}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Retry" }));

    // Assert
    expect(screen.queryByText("Group not found")).toBeNull();
    expect(screen.getByText("Server error")).toBeTruthy();
    expect(onRetry).toHaveBeenCalledOnce();
  });
});
