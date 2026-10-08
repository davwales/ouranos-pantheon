import { parseSegments } from "@/components/shared/ai-assistant/parse-segments";
import { describe, expect, it } from "vitest";

describe("parseSegments", () => {
  it("WhenEmptyAndStreaming_ShouldReturnLoading", () => {
    // Arrange & Act
    const segments = parseSegments("", true);

    // Assert
    expect(segments).toEqual([{ type: "loading" }]);
  });

  it("WhenEmptyAndNotStreaming_ShouldReturnNothing", () => {
    // Arrange & Act
    const segments = parseSegments("", false);

    // Assert
    expect(segments).toEqual([]);
  });

  it("WhenPlainText_ShouldReturnTextSegment", () => {
    // Arrange & Act
    const segments = parseSegments("Hello world", false);

    // Assert
    expect(segments).toEqual([{ type: "text", content: "Hello world" }]);
  });

  it("WhenClosedThinkBlock_ShouldSplitThinkingAndText", () => {
    // Arrange & Act
    const segments = parseSegments("<think>plan</think>Answer", true);

    // Assert
    expect(segments).toEqual([
      { type: "thinking", content: "plan", isStreaming: false },
      { type: "text", content: "Answer" },
    ]);
  });

  it("WhenUnclosedThinkBlockWhileStreaming_ShouldMarkThinkingAsStreaming", () => {
    // Arrange & Act
    const segments = parseSegments("Intro <think>still going", true);

    // Assert
    expect(segments).toEqual([
      { type: "text", content: "Intro " },
      { type: "thinking", content: "still going", isStreaming: true },
    ]);
  });

  it("WhenPartialOpenTag_ShouldStripIt", () => {
    // Arrange & Act
    const segments = parseSegments("Hello <thi", true);

    // Assert
    expect(segments).toEqual([{ type: "text", content: "Hello " }]);
  });
});
