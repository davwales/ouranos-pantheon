import {
  toChatParts,
  toMessageParts,
  toolCallsOf,
  withEditedText,
} from "@/app/(hermes)/hermes/components/message-parts";
import { type AssistantChatPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { type MessagePart } from "@/lib/api/hermes";
import { describe, expect, it } from "vitest";

const TOOL_CALL: MessagePart = {
  $type: "toolCall",
  callId: "call-1",
  name: "echo",
  arguments: '{"text":"hi"}',
  status: "Completed",
  output: "echo: hi",
  isTruncated: false,
  durationMs: 1200,
};

describe("message parts", () => {
  it("toChatParts_WhenPartsAreSaved_ShouldRenderThemAsFinishedChatParts", () => {
    // Arrange
    const parts: MessagePart[] = [
      { $type: "reasoning", content: "Thinking" },
      TOOL_CALL,
      { $type: "text", content: "It said hi." },
    ];

    // Act
    const result = toChatParts(parts);

    // Assert
    expect(result).toEqual([
      { kind: "reasoning", id: "part-0", content: "Thinking", endedAt: 0 },
      {
        kind: "tool",
        id: "part-1",
        callId: "call-1",
        name: "echo",
        arguments: '{"text":"hi"}',
        status: "completed",
        output: "echo: hi",
        isTruncated: false,
        durationMs: 1200,
      },
      { kind: "text", id: "part-2", content: "It said hi." },
    ]);
  });

  it("toChatParts_WhenAToolCallHasNoDuration_ShouldLeaveItUnknown", () => {
    // Arrange
    const legacy: MessagePart = { ...TOOL_CALL, durationMs: null };

    // Act
    const [part] = toChatParts([legacy]);

    // Assert
    expect(part).not.toHaveProperty("durationMs", expect.anything());
    expect(toMessageParts([part])).toEqual([{ ...TOOL_CALL, durationMs: undefined }]);
  });

  it("toMessageParts_WhenToolNeverFinished_ShouldSaveItAsFailedAndDropActions", () => {
    // Arrange
    const parts: AssistantChatPart[] = [
      {
        kind: "tool",
        id: "t",
        callId: "call-1",
        name: "echo",
        arguments: "{}",
        status: "cancelled",
        startedAt: 1,
      },
      {
        kind: "action",
        id: "a",
        callId: "call-2",
        name: "set_view",
        action: {},
        status: "proposed",
        arguments: "{}",
      },
      { kind: "text", id: "x", content: "Done" },
    ];

    // Act
    const result = toMessageParts(parts);

    // Assert
    expect(result).toEqual([
      {
        $type: "toolCall",
        callId: "call-1",
        name: "echo",
        arguments: "{}",
        status: "Failed",
        output: "",
        isTruncated: false,
      },
      { $type: "text", content: "Done" },
    ]);
  });

  it("toMessageParts_WhenRoundTripped_ShouldKeepSavedParts", () => {
    // Arrange
    const parts: MessagePart[] = [
      { $type: "reasoning", content: "Thinking" },
      { ...TOOL_CALL, status: "Skipped", isTruncated: true },
      { $type: "text", content: "Ok" },
    ];

    // Act
    const result = toMessageParts(toChatParts(parts));

    // Assert
    expect(result).toEqual(parts);
  });

  it("toolCallsOf_WhenMessageHasToolCalls_ShouldReturnTheReplayPayload", () => {
    // Arrange & Act
    const result = toolCallsOf([{ $type: "reasoning", content: "x" }, TOOL_CALL]);

    // Assert
    expect(result).toEqual([
      { callId: "call-1", name: "echo", arguments: '{"text":"hi"}', output: "echo: hi" },
    ]);
  });

  it("toolCallsOf_WhenMessageHasNoToolCalls_ShouldReturnUndefined", () => {
    // Arrange & Act & Assert
    expect(toolCallsOf(undefined)).toBeUndefined();
    expect(toolCallsOf([{ $type: "text", content: "Hi" }])).toBeUndefined();
  });

  it("withEditedText_WhenReplyHasParts_ShouldReplaceTextAndKeepTheRest", () => {
    // Arrange
    const parts: MessagePart[] = [
      { $type: "reasoning", content: "Thinking" },
      { $type: "text", content: "Before" },
      TOOL_CALL,
      { $type: "text", content: "After" },
    ];

    // Act
    const result = withEditedText(parts, "Edited");

    // Assert
    expect(result).toEqual([
      { $type: "reasoning", content: "Thinking" },
      TOOL_CALL,
      { $type: "text", content: "Edited" },
    ]);
  });

  it("withEditedText_WhenMessageHasNoParts_ShouldLeaveThemAlone", () => {
    // Arrange & Act & Assert
    expect(withEditedText(undefined, "Edited")).toBeUndefined();
    expect(withEditedText([], "Edited")).toEqual([]);
  });

  it("toChatParts_WhenAnImageIsSaved_ShouldPointAtTheGatewayAndRoundTrip", () => {
    // Arrange
    const parts: MessagePart[] = [
      { $type: "image", imageId: "img-1", width: 64, height: 32, altText: "A fox" },
    ];

    // Act
    const chatParts = toChatParts(parts);

    // Assert
    expect(chatParts[0]).toEqual(
      expect.objectContaining({ kind: "image", url: "/api/hermes/images/img-1" }),
    );
    expect(toMessageParts(chatParts)).toEqual(parts);
  });
});
