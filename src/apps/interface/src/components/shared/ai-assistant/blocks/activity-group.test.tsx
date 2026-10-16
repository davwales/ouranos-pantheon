import {
  ActivityGroup,
  type ActivityPart,
} from "@/components/shared/ai-assistant/blocks/activity-group";
import { type AssistantChatToolPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

function tools(count: number, update: Partial<AssistantChatToolPart> = {}): AssistantChatToolPart[] {
  return Array.from({ length: count }, (_, i) => ({
    kind: "tool",
    id: `p${i}`,
    callId: `c${i}`,
    name: `tool_${i}`,
    arguments: "{}",
    status: "completed",
    startedAt: i * 1000,
    endedAt: i * 1000 + 500,
    ...update,
  }));
}

describe("ActivityGroup", () => {
  it("WhenTheRunIsShort_ShouldListEveryRow", () => {
    // Arrange & Act
    render(<ActivityGroup parts={tools(3)} isActive={false} />);

    // Assert
    expect(screen.getByText("tool_0")).toBeInTheDocument();
    expect(screen.getByText("tool_2")).toBeInTheDocument();
    expect(screen.queryByText(/Used/)).not.toBeInTheDocument();
  });

  it("WhenALongRunSettled_ShouldFoldIntoASummaryWithTheTotalTime", () => {
    // Arrange
    const parts: ActivityPart[] = [
      ...tools(2),
      { kind: "reasoning", id: "r1", content: "Hmm", startedAt: 2000, endedAt: 3000 },
      ...tools(1).map((tool) => ({ ...tool, id: "p9", name: "tool_9", startedAt: 3000, endedAt: 3500 })),
    ];
    render(<ActivityGroup parts={parts} isActive={false} />);

    // Act
    const summary = screen.getByRole("button", { name: /Used 3 tools/ });

    // Assert
    expect(summary).toHaveTextContent("3.5s");
    expect(screen.queryByText("tool_0")).not.toBeInTheDocument();
    fireEvent.click(summary);
    expect(screen.getByText("tool_0")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Thought for 1s" })).toBeInTheDocument();
  });

  it("WhenACallFailed_ShouldSayHowManyInTheSummary", () => {
    // Arrange
    const parts = tools(4);
    parts[1] = { ...parts[1], status: "failed" };

    // Act
    render(<ActivityGroup parts={parts} isActive={false} />);

    // Assert
    expect(screen.getByRole("button", { name: /Used 4 tools/ })).toHaveTextContent("1 failed");
  });

  it("WhenTheModelIsStillWorkingThroughTheRun_ShouldKeepEveryRowVisible", () => {
    // Arrange & Act
    render(<ActivityGroup parts={tools(5)} isActive />);

    // Assert
    expect(screen.queryByText(/Used/)).not.toBeInTheDocument();
    expect(screen.getByText("tool_4")).toBeInTheDocument();
  });

  it("WhenAToolIsRunning_ShouldKeepEveryRowVisible", () => {
    // Arrange & Act
    render(<ActivityGroup parts={tools(5, { status: "running" })} isActive={false} />);

    // Assert
    expect(screen.queryByText(/Used/)).not.toBeInTheDocument();
  });

  it("WhenReasoningIsStreaming_ShouldShowThinkingOnItsRow", () => {
    // Arrange & Act
    render(
      <ActivityGroup
        parts={[...tools(1), { kind: "reasoning", id: "r1", content: "Hmm", startedAt: 0 }]}
        isActive
        streamingId="r1"
      />,
    );

    // Assert
    expect(screen.getByText("Thinking…")).toBeInTheDocument();
  });
});
