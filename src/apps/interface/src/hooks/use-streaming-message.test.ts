import { act, renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { type StreamingChunk, useStreamingMessage } from "./use-streaming-message";

async function* chunks(...items: StreamingChunk[]): AsyncGenerator<StreamingChunk> {
  for (const item of items) {
    yield item;
  }
}

describe("useStreamingMessage", () => {
  it("dispatches an error chunk to onErrorMessage", async () => {
    const onErrorMessage = vi.fn();
    const onComplete = vi.fn();
    const { result } = renderHook(() => useStreamingMessage());

    await act(async () => {
      await result.current.startStream({
        generator: () => chunks({ $type: "error", message: "Nothing to compact." }),
        chunkHandlers: { onErrorMessage, onComplete },
      });
    });

    expect(onErrorMessage).toHaveBeenCalledWith("Nothing to compact.");
    expect(onComplete).not.toHaveBeenCalled();
    expect(result.current.isStreaming).toBe(false);
  });

  it("dispatches content, usage and complete chunks", async () => {
    const onContent = vi.fn();
    const onUsage = vi.fn();
    const onComplete = vi.fn();
    const { result } = renderHook(() => useStreamingMessage());

    await act(async () => {
      await result.current.startStream({
        generator: () =>
          chunks(
            { $type: "content", content: "Summary." },
            { $type: "usage", inputTokens: 10, outputTokens: 5, totalTokens: 15 },
            { $type: "complete", summaryMessageId: null },
          ),
        chunkHandlers: { onContent, onUsage, onComplete },
      });
    });

    expect(onContent).toHaveBeenCalledWith("Summary.");
    expect(onUsage).toHaveBeenCalledWith({ inputTokens: 10, outputTokens: 5, totalTokens: 15 });
    expect(onComplete).toHaveBeenCalledOnce();
  });
});
