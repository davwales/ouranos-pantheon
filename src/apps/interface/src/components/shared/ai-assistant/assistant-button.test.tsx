import { AssistantButton } from "@/components/shared/ai-assistant/assistant-button";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/hooks/use-mobile", () => ({
  useIsMobile: () => false,
}));

vi.mock("@/lib/api/assistant", () => ({
  streamAssistant: vi.fn(),
}));

describe("AssistantButton", () => {
  it("WhenRendered_ShouldNotOpenPanel", () => {
    // Arrange & Act
    render(
      <AssistantButton
        title="Test Assistant"
        endpoint="/api/test/assistant/completions/stream"
        context={{ id: "1" }}
      />,
    );

    // Assert
    expect(screen.getByRole("button", { name: "Open Test Assistant" })).toBeInTheDocument();
    expect(screen.queryByText("Test Assistant")).not.toBeInTheDocument();
  });

  it("WhenClicked_ShouldOpenPanel", async () => {
    // Arrange
    render(
      <AssistantButton
        title="Test Assistant"
        endpoint="/api/test/assistant/completions/stream"
        context={{ id: "1" }}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Open Test Assistant" }));

    // Assert
    expect(await screen.findByText("Test Assistant", { selector: "h2" })).toBeInTheDocument();
    expect(screen.getByLabelText("Message")).toBeInTheDocument();
  });
});
