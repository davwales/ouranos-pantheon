import { Message } from "@/components/shared/ai-assistant/message";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

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

  it("WhenToolsWereUsed_ShouldShowTheirLabelsOnce", () => {
    // Arrange & Act
    render(
      <Message
        name="Assistant"
        role="Assistant"
        content="Up 5%."
        tools={["get_price_history", "get_price_history", "get_signals"]}
        toolLabels={{ get_signals: "signals" }}
      />,
    );

    // Assert
    expect(screen.getByText("Looked up price history, signals")).toBeInTheDocument();
  });

  it("WhenActionIsProposed_ShouldDescribeItAndApplyOnClick", () => {
    // Arrange
    const onApplyAction = vi.fn();
    render(
      <Message
        name="Assistant"
        role="Assistant"
        content="Proposed."
        actions={[
          { id: "a1", name: "add_note", action: { text: "Rest it" }, status: "proposed" },
          { id: "a2", name: "unknown", action: {}, status: "proposed" },
        ]}
        actionHandlers={{
          add_note: {
            describe: (action) => `Add note: ${(action as { text: string }).text}`,
            apply: vi.fn(),
          },
        }}
        onApplyAction={onApplyAction}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Apply" }));

    // Assert
    expect(screen.getByText("Add note: Rest it")).toBeInTheDocument();
    expect(screen.getAllByRole("button", { name: "Apply" })).toHaveLength(1);
    expect(onApplyAction).toHaveBeenCalledWith("a1");
  });

  it("WhenActionIsApplied_ShouldShowAppliedInsteadOfTheButton", () => {
    // Arrange & Act
    render(
      <Message
        name="Assistant"
        role="Assistant"
        content="Done."
        actions={[{ id: "a1", name: "set_view", action: {}, status: "applied" }]}
        actionHandlers={{ set_view: { describe: () => "Sort by ROI", apply: vi.fn() } }}
      />,
    );

    // Assert
    expect(screen.getByText("Applied")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Apply" })).not.toBeInTheDocument();
  });
});
