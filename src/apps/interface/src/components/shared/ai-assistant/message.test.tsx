import { Message } from "@/components/shared/ai-assistant/message";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

describe("Message", () => {
  it("WhenFromTheUser_ShouldRenderABubbleWithoutAName", () => {
    // Arrange & Act
    render(<Message name="Assistant" role="User" content="Is it a buy?" />);

    // Assert
    expect(screen.getByText("Is it a buy?")).toBeInTheDocument();
    expect(screen.queryByText("Assistant")).not.toBeInTheDocument();
  });

  it("WhenFromTheAssistantWithParts_ShouldRenderTheTurnWithItsName", () => {
    // Arrange & Act
    render(
      <Message
        name="Kitchen Assistant"
        role="Assistant"
        content="Use butter."
        parts={[{ kind: "text", id: "t1", content: "Use butter." }]}
      />,
    );

    // Assert
    expect(screen.getByText("Kitchen Assistant")).toBeInTheDocument();
    expect(screen.getByText("Use butter.")).toBeInTheDocument();
  });

  it("WhenContentHasThinkTagsAndNoParts_ShouldSplitReasoningFromTheReply", () => {
    // Arrange & Act
    render(
      <Message name="Persona" role="Assistant" content="<think>Weighing it</think>Use butter." />,
    );

    // Assert
    expect(screen.getByRole("button", { name: "Reasoning" })).toBeInTheDocument();
    expect(screen.getByText("Use butter.")).toBeInTheDocument();
    expect(screen.queryByText("Weighing it")).not.toBeInTheDocument();
  });

  it("WhenThinkTagIsStillOpenWhileStreaming_ShouldShowThinking", () => {
    // Arrange & Act
    render(<Message name="Persona" role="Assistant" content="<think>Weighing" isStreaming />);

    // Assert
    expect(screen.getByText("Thinking…")).toBeInTheDocument();
  });

  it("WhenReasoningIsToggledInsideAClickableMessage_ShouldNotTriggerTheMessage", () => {
    // Arrange
    const onMessageClick = vi.fn();
    render(
      <button type="button" onClick={onMessageClick}>
        <Message name="Persona" role="Assistant" content="<think>Weighing it</think>Use butter." />
      </button>,
    );

    // Act
    fireEvent.click(screen.getByText("Reasoning"));
    fireEvent.keyDown(screen.getByText("Reasoning"), { key: " " });

    // Assert
    expect(onMessageClick).not.toHaveBeenCalled();
    expect(screen.queryByText("Weighing it")).not.toBeInTheDocument();
  });
});
