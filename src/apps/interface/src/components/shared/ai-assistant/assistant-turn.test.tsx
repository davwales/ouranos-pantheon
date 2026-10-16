import { AssistantTurn } from "@/components/shared/ai-assistant/assistant-turn";
import {
  type AssistantChatActionPart,
  type AssistantChatToolPart,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

function tool(id: string, update: Partial<AssistantChatToolPart> = {}): AssistantChatToolPart {
  return {
    kind: "tool",
    id,
    callId: id,
    name: `tool_${id}`,
    arguments: "{}",
    status: "completed",
    output: "ok",
    startedAt: 0,
    endedAt: 400,
    ...update,
  };
}

function action(id: string, update: Partial<AssistantChatActionPart> = {}): AssistantChatActionPart {
  return {
    kind: "action",
    id,
    callId: id,
    name: "add_note",
    action: { text: id },
    status: "proposed",
    arguments: "{}",
    ...update,
  };
}

const HANDLERS = {
  add_note: {
    describe: (value: unknown) => `Add note ${(value as { text: string }).text}`,
    apply: vi.fn(),
  },
};

describe("AssistantTurn", () => {
  it("WhenStreamingWithNoParts_ShouldShowThatTheAssistantIsThinking", () => {
    // Arrange & Act
    render(<AssistantTurn name="Assistant" parts={[]} isStreaming />);

    // Assert
    expect(screen.getByRole("status")).toHaveTextContent("Thinking…");
  });

  it("WhenStreamingAfterAToolSettled_ShouldShowThatTheAssistantIsThinking", () => {
    // Arrange & Act
    render(<AssistantTurn name="Assistant" parts={[tool("a")]} isStreaming />);

    // Assert
    expect(screen.getByRole("status")).toBeInTheDocument();
  });

  it("WhenAToolIsRunning_ShouldNotShowThePendingIndicator", () => {
    // Arrange & Act
    render(
      <AssistantTurn name="Assistant" parts={[tool("a", { status: "running" })]} isStreaming />,
    );

    // Assert
    expect(screen.queryByRole("status")).not.toBeInTheDocument();
    expect(screen.getByText("Running…")).toBeInTheDocument();
  });

  it("WhenReasoningHasNoTools_ShouldRenderItInline", () => {
    // Arrange & Act
    const { container } = render(
      <AssistantTurn
        name="Assistant"
        parts={[
          { kind: "reasoning", id: "r1", content: "Hmm", startedAt: 0, endedAt: 2000 },
          { kind: "text", id: "t1", content: "Answer" },
        ]}
      />,
    );

    // Assert
    expect(screen.getByRole("button", { name: "Thought for 2s" })).toBeInTheDocument();
    expect(container.querySelector(".rounded-lg")).toBeNull();
  });

  it("WhenReasoningSitsBetweenTools_ShouldStackThemInOneActivityGroup", () => {
    // Arrange & Act
    render(
      <AssistantTurn
        name="Assistant"
        parts={[
          tool("a"),
          { kind: "reasoning", id: "r1", content: "Hmm", startedAt: 0, endedAt: 1000 },
          tool("b"),
        ]}
      />,
    );

    // Assert
    const group = screen.getByText("tool_a").closest(".rounded-lg");
    expect(group).toBe(screen.getByText("tool_b").closest(".rounded-lg"));
    expect(group).toContainElement(screen.getByRole("button", { name: "Thought for 1s" }));
  });

  it("WhenPartsInterleave_ShouldRenderThemInOrderWithConsecutiveToolsGrouped", () => {
    // Arrange & Act
    const { container } = render(
      <AssistantTurn
        name="Assistant"
        parts={[
          { kind: "text", id: "t1", content: "First" },
          tool("a"),
          tool("b"),
          { kind: "text", id: "t2", content: "Second" },
        ]}
      />,
    );

    // Assert
    const text = container.textContent ?? "";
    expect(text.indexOf("First")).toBeLessThan(text.indexOf("tool_a"));
    expect(text.indexOf("tool_b")).toBeLessThan(text.indexOf("Second"));
    expect(screen.getByText("tool_a").closest(".rounded-lg")).toBe(
      screen.getByText("tool_b").closest(".rounded-lg"),
    );
  });

  it("WhenGivenTheTurnContext_ShouldPassItToToolInputs", () => {
    // Arrange
    render(<AssistantTurn name="Assistant" parts={[tool("a")]} context={{ symbolId: "s1" }} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /tool_a/ }));

    // Assert
    expect(screen.getByRole("region", { name: "Input" })).toHaveTextContent('"symbolId": "s1"');
  });

  it("WhenSeveralProposalsAreOpen_ShouldOfferApplyAllAndRejectAll", () => {
    // Arrange
    const onApplyAll = vi.fn();
    const onRejectAll = vi.fn();
    render(
      <AssistantTurn
        name="Assistant"
        parts={[action("a"), action("b"), action("c", { status: "applied" })]}
        actionHandlers={HANDLERS}
        onApplyAll={onApplyAll}
        onRejectAll={onRejectAll}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Apply all" }));
    fireEvent.click(screen.getByRole("button", { name: "Reject all" }));

    // Assert
    expect(screen.getByText("2 changes proposed")).toBeInTheDocument();
    expect(onApplyAll).toHaveBeenCalledOnce();
    expect(onRejectAll).toHaveBeenCalledOnce();
  });

  it("WhenOnlyOneProposalIsOpen_ShouldNotOfferApplyAll", () => {
    // Arrange & Act
    render(
      <AssistantTurn
        name="Assistant"
        parts={[action("a")]}
        actionHandlers={HANDLERS}
        onApplyAll={vi.fn()}
        onRejectAll={vi.fn()}
      />,
    );

    // Assert
    expect(screen.queryByRole("button", { name: "Apply all" })).not.toBeInTheDocument();
  });

  it("WhenAnActionIsApplied_ShouldCallBackWithItsId", () => {
    // Arrange
    const onApplyAction = vi.fn();
    const onRejectAction = vi.fn();
    render(
      <AssistantTurn
        name="Assistant"
        parts={[action("a")]}
        actionHandlers={HANDLERS}
        onApplyAction={onApplyAction}
        onRejectAction={onRejectAction}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Apply" }));
    fireEvent.click(screen.getByRole("button", { name: "Reject" }));

    // Assert
    expect(onApplyAction).toHaveBeenCalledWith("a");
    expect(onRejectAction).toHaveBeenCalledWith("a");
  });
});
