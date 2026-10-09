import { Message } from "@/components/shared/ai-assistant/message";
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

describe("Message", () => {
  it("WhenReasoningStreamsBeforeContent_ShouldShowThinking", () => {
    // Arrange & Act
    render(
      <Message
        name="Assistant"
        role="Assistant"
        content=""
        reasoning="Weighing options"
        isStreaming
      />,
    );

    // Assert
    expect(screen.getByText("Thinking...")).toBeInTheDocument();
  });

  it("WhenContentFollowsReasoning_ShouldShowReasoningAndContent", () => {
    // Arrange & Act
    render(
      <Message
        name="Assistant"
        role="Assistant"
        content="Use butter."
        reasoning="Weighing options"
        isStreaming
      />,
    );

    // Assert
    expect(screen.getByText("Reasoning")).toBeInTheDocument();
    expect(screen.getByText("Use butter.")).toBeInTheDocument();
  });

  it("WhenNoReasoning_ShouldRenderContentOnly", () => {
    // Arrange & Act
    render(<Message name="Assistant" role="Assistant" content="Hello" />);

    // Assert
    expect(screen.queryByText("Reasoning")).not.toBeInTheDocument();
    expect(screen.getByText("Hello")).toBeInTheDocument();
  });
});
