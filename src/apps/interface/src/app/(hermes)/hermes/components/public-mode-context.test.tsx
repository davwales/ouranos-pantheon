import {
  PublicModeProvider,
  usePublicMode,
} from "@/app/(hermes)/hermes/components/public-mode-context";
import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const getPublicMode = vi.fn();

vi.mock("@/lib/api/hermes", () => ({
  hermesApi: { getPublicMode: () => getPublicMode() },
}));

function Probe() {
  return <span>{usePublicMode() ? "public" : "private"}</span>;
}

describe("PublicModeProvider", () => {
  beforeEach(() => {
    getPublicMode.mockReset();
  });

  it("WhenTheGatewayReportsPublicModeOff_ShouldProvideFalse", async () => {
    // Arrange
    getPublicMode.mockResolvedValue({ isEnabled: false });

    // Act
    render(
      <PublicModeProvider>
        <Probe />
      </PublicModeProvider>,
    );

    // Assert
    expect(await screen.findByText("private")).toBeInTheDocument();
  });

  it("WhileLoading_ShouldProvideTrue", () => {
    // Arrange
    getPublicMode.mockReturnValue(new Promise(() => {}));

    // Act
    render(
      <PublicModeProvider>
        <Probe />
      </PublicModeProvider>,
    );

    // Assert
    expect(screen.getByText("public")).toBeInTheDocument();
  });

  it("WithoutAProvider_ShouldProvideFalse", () => {
    // Arrange & Act
    render(<Probe />);

    // Assert
    expect(screen.getByText("private")).toBeInTheDocument();
  });
});
