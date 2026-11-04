import {
  FeatureFlagsProvider,
  useFeatureFlag,
} from "@/components/shared/feature-flags-context";
import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const getAll = vi.fn();

vi.mock("@/lib/api/feature-flags", () => ({
  featureFlagsApi: { getAll: () => getAll() },
}));

function Probe({
  name,
  whileUnknown,
}: {
  name: string;
  whileUnknown?: boolean;
}) {
  return <span>{useFeatureFlag(name, { whileUnknown }) ? "on" : "off"}</span>;
}

describe("FeatureFlagsProvider", () => {
  beforeEach(() => {
    getAll.mockReset();
  });

  it("WhenTheGatewayReportsTheFlag_ShouldProvideItsState", async () => {
    // Arrange
    getAll.mockResolvedValue({ flags: { "module.flag": true } });

    // Act
    render(
      <FeatureFlagsProvider>
        <Probe name="module.flag" />
      </FeatureFlagsProvider>,
    );

    // Assert
    expect(await screen.findByText("on")).toBeInTheDocument();
  });

  it("WhenTheGatewayDoesNotReportTheFlag_ShouldProvideOffEvenWhileUnknownIsOn", async () => {
    // Arrange
    getAll.mockResolvedValue({ flags: {} });

    // Act
    render(
      <FeatureFlagsProvider>
        <Probe name="module.flag" whileUnknown />
      </FeatureFlagsProvider>,
    );

    // Assert
    expect(await screen.findByText("off")).toBeInTheDocument();
  });

  it("WhileLoading_ShouldProvideWhileUnknown", () => {
    // Arrange
    getAll.mockReturnValue(new Promise(() => {}));

    // Act
    render(
      <FeatureFlagsProvider>
        <Probe name="module.flag" whileUnknown />
      </FeatureFlagsProvider>,
    );

    // Assert
    expect(screen.getByText("on")).toBeInTheDocument();
  });

  it("WhenTheRequestFails_ShouldProvideWhileUnknown", async () => {
    // Arrange
    getAll.mockRejectedValue(new Error("unreachable"));

    // Act
    render(
      <FeatureFlagsProvider>
        <Probe name="module.flag" whileUnknown />
      </FeatureFlagsProvider>,
    );

    // Assert
    await vi.waitFor(() => expect(getAll).toHaveBeenCalled());
    expect(screen.getByText("on")).toBeInTheDocument();
  });

  it("WithoutAProvider_ShouldProvideOff", () => {
    // Arrange & Act
    render(<Probe name="module.flag" whileUnknown />);

    // Assert
    expect(screen.getByText("off")).toBeInTheDocument();
  });
});
