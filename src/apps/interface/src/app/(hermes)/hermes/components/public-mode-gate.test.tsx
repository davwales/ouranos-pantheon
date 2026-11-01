import { PublicModeGate } from "@/app/(hermes)/hermes/components/public-mode-gate";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const isPublicMode = vi.hoisted(() => ({ value: false }));

vi.mock("@/app/(hermes)/hermes/components/public-mode-context", () => ({
  usePublicMode: () => isPublicMode.value,
}));

describe("PublicModeGate", () => {
  it("WhenPublicModeIsOff_ShouldRenderChildren", () => {
    // Arrange
    isPublicMode.value = false;

    // Act
    render(
      <PublicModeGate backHref="/hermes/traits" backLabel="Back to Traits">
        <p>Form</p>
      </PublicModeGate>,
    );

    // Assert
    expect(screen.getByText("Form")).toBeInTheDocument();
  });

  it("WhenPublicModeIsOn_ShouldExplainAndLinkBack", () => {
    // Arrange
    isPublicMode.value = true;

    // Act
    render(
      <PublicModeGate backHref="/hermes/traits" backLabel="Back to Traits">
        <p>Form</p>
      </PublicModeGate>,
    );

    // Assert
    expect(screen.queryByText("Form")).not.toBeInTheDocument();
    expect(screen.getByText(/public mode/)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Back to Traits" })).toHaveAttribute(
      "href",
      "/hermes/traits",
    );
  });
});
