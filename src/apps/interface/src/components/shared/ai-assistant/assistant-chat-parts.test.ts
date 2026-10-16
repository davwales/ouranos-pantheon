import {
  actionOutcome,
  applyEvent,
  finishParts,
  textOf,
  type AssistantChatPart,
  type StreamedAssistantEvent,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import { describe, expect, it } from "vitest";

function run(...events: StreamedAssistantEvent[]): AssistantChatPart[] {
  return events.reduce<AssistantChatPart[]>(
    (parts, event, i) => applyEvent(parts, event, i * 100, `p${i}`),
    [],
  );
}

describe("assistant chat parts", () => {
  it("applyEvent_WhenReasoningAndContentStream_ShouldMergeRunsAndCloseReasoningWhenTextStarts", () => {
    // Arrange & Act
    const parts = run(
      { $type: "reasoning", content: "Let me" },
      { $type: "reasoning", content: " think" },
      { $type: "content", content: "Hello" },
      { $type: "content", content: " there" },
    );

    // Assert
    expect(parts).toEqual([
      { kind: "reasoning", id: "p0", content: "Let me think", startedAt: 0, endedAt: 200 },
      { kind: "text", id: "p2", content: "Hello there" },
    ]);
  });

  it("applyEvent_WhenAToolSettles_ShouldRecordItsOutcomeAndDuration", () => {
    // Arrange & Act
    const parts = run(
      { $type: "tool", callId: "c1", name: "get_signals", arguments: "{}" },
      { $type: "toolResult", callId: "c1", status: "Skipped", output: "Skipped.", isTruncated: true },
    );

    // Assert
    expect(parts).toEqual([
      {
        kind: "tool",
        id: "p0",
        callId: "c1",
        name: "get_signals",
        arguments: "{}",
        status: "skipped",
        output: "Skipped.",
        isTruncated: true,
        startedAt: 0,
        endedAt: 100,
      },
    ]);
  });

  it("applyEvent_WhenCallIdsRepeatAcrossRounds_ShouldSettleTheNewestCall", () => {
    // Arrange & Act
    const parts = run(
      { $type: "tool", callId: "c1", name: "first", arguments: "{}" },
      { $type: "toolResult", callId: "c1", status: "Completed", output: "1", isTruncated: false },
      { $type: "tool", callId: "c1", name: "second", arguments: "{}" },
      { $type: "toolResult", callId: "c1", status: "Failed", output: "2", isTruncated: false },
    );

    // Assert
    expect(parts.map((part) => part.kind === "tool" && part.status)).toEqual([
      "completed",
      "failed",
    ]);
  });

  it("applyEvent_WhenAToolProposesAnAction_ShouldReplaceTheToolWithTheAction", () => {
    // Arrange & Act
    const parts = run(
      { $type: "tool", callId: "c1", name: "add_note", arguments: '{"text":"A"}' },
      { $type: "action", callId: "c1", name: "add_note", action: { text: "A" } },
      { $type: "toolResult", callId: "c1", status: "Completed", output: "Proposed", isTruncated: false },
    );

    // Assert
    expect(parts).toEqual([
      {
        kind: "action",
        id: "p1",
        callId: "c1",
        name: "add_note",
        action: { text: "A" },
        status: "proposed",
        arguments: '{"text":"A"}',
        output: "Proposed",
        isTruncated: false,
      },
    ]);
  });

  it("applyEvent_WhenAnActionHasNoCall_ShouldAppendIt", () => {
    // Arrange & Act
    const parts = run(
      { $type: "reasoning", content: "Hmm" },
      { $type: "action", callId: "c9", name: "add_note", action: {} },
    );

    // Assert
    expect(parts).toEqual([
      { kind: "reasoning", id: "p0", content: "Hmm", startedAt: 0, endedAt: 100 },
      expect.objectContaining({ kind: "action", callId: "c9", arguments: "" }),
    ]);
  });

  it("applyEvent_WhenAResultHasNoCall_ShouldIgnoreIt", () => {
    // Arrange & Act
    const parts = run(
      { $type: "content", content: "Hi" },
      { $type: "toolResult", callId: "c1", status: "Completed", output: "1", isTruncated: false },
    );

    // Assert
    expect(parts).toEqual([{ kind: "text", id: "p0", content: "Hi" }]);
  });

  it("finishParts_ShouldCloseReasoningAndCancelRunningTools", () => {
    // Arrange
    const parts = run(
      { $type: "tool", callId: "c1", name: "get_signals", arguments: "{}" },
      { $type: "reasoning", content: "Hmm" },
    );

    // Act
    const finished = finishParts(parts, 500);

    // Assert
    expect(finished).toEqual([
      expect.objectContaining({ kind: "tool", status: "cancelled", endedAt: 500 }),
      expect.objectContaining({ kind: "reasoning", endedAt: 500 }),
    ]);
  });

  it("textOf_ShouldJoinTheTextParts", () => {
    // Arrange
    const parts = run(
      { $type: "content", content: "One" },
      { $type: "tool", callId: "c1", name: "t", arguments: "{}" },
      { $type: "content", content: "Two" },
    );

    // Act & Assert
    expect(textOf(parts)).toBe("One\n\nTwo");
  });

  it("actionOutcome_ShouldMapEveryStatus", () => {
    // Act & Assert
    expect(
      (["proposed", "applying", "applied", "rejected", "failed"] as const).map(actionOutcome),
    ).toEqual(["Pending", "Pending", "Applied", "Rejected", "Failed"]);
  });
});
