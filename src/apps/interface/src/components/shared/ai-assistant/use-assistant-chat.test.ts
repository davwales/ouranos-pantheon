import { useAssistantChat } from "@/components/shared/ai-assistant/use-assistant-chat";
import {
  streamAssistant,
  type AssistantEndpoint,
  type AssistantEvent,
} from "@/lib/api/assistant";
import { act, renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/lib/api/assistant", () => ({
  streamAssistant: vi.fn(),
}));

const CONTEXT = { recipeId: "abc" };
const ENDPOINT: AssistantEndpoint<typeof CONTEXT> =
  "/api/test/assistant/completions/stream";

async function* events(...items: AssistantEvent[]): AsyncGenerator<AssistantEvent> {
  for (const item of items) {
    yield item;
  }
}

function renderChat() {
  return renderHook(() =>
    useAssistantChat({ endpoint: ENDPOINT, context: CONTEXT }),
  );
}

describe("useAssistantChat", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("WhenSendSucceeds_ShouldAccumulateContentAndReturnToIdle", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "content", content: "Hello" },
        { $type: "content", content: " there" },
        { $type: "done" },
      ),
    );
    const { result } = renderChat();

    // Act
    await act(() => result.current.send("Hi"));

    // Assert
    expect(result.current.isStreaming).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.messages.map(({ role, content }) => ({ role, content }))).toEqual([
      { role: "User", content: "Hi" },
      { role: "Assistant", content: "Hello there" },
    ]);
    expect(streamAssistant).toHaveBeenCalledWith(
      ENDPOINT,
      { messages: [{ role: "User", content: "Hi" }], context: CONTEXT },
      expect.any(AbortSignal),
    );
  });

  it("WhenReasoningIsStreamed_ShouldAccumulateItSeparatelyAndNotResendIt", async () => {
    // Arrange
    vi.mocked(streamAssistant)
      .mockReturnValueOnce(
        events(
          { $type: "reasoning", content: "Let me" },
          { $type: "reasoning", content: " think" },
          { $type: "content", content: "Answer" },
          { $type: "done" },
        ),
      )
      .mockReturnValueOnce(events({ $type: "content", content: "Second" }));
    const { result } = renderChat();
    await act(() => result.current.send("One"));

    // Act
    await act(() => result.current.send("Two"));

    // Assert
    expect(result.current.messages[1]).toMatchObject({
      role: "Assistant",
      content: "Answer",
      reasoning: "Let me think",
    });
    expect(vi.mocked(streamAssistant).mock.calls[1][1].messages).toEqual([
      { role: "User", content: "One" },
      { role: "Assistant", content: "Answer" },
      { role: "User", content: "Two" },
    ]);
  });

  it("WhenSendingFollowUp_ShouldIncludeHistory", async () => {
    // Arrange
    vi.mocked(streamAssistant)
      .mockReturnValueOnce(events({ $type: "content", content: "First" }))
      .mockReturnValueOnce(events({ $type: "content", content: "Second" }));
    const { result } = renderChat();
    await act(() => result.current.send("One"));

    // Act
    await act(() => result.current.send("Two"));

    // Assert
    expect(vi.mocked(streamAssistant).mock.calls[1][1].messages).toEqual([
      { role: "User", content: "One" },
      { role: "Assistant", content: "First" },
      { role: "User", content: "Two" },
    ]);
  });

  it("WhenStreamClosesWithoutDone_ShouldStillComplete", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events({ $type: "content", content: "Partial" }),
    );
    const { result } = renderChat();

    // Act
    await act(() => result.current.send("Hi"));

    // Assert
    expect(result.current.isStreaming).toBe(false);
    expect(result.current.messages.at(-1)?.content).toBe("Partial");
  });

  it("WhenErrorEventReceivedBeforeContent_ShouldWithdrawTurn", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events({ $type: "error", message: "The assistant failed." }),
    );
    const { result } = renderChat();

    // Act
    let sent = true;
    await act(async () => {
      sent = await result.current.send("Hi");
    });

    // Assert
    expect(sent).toBe(false);
    expect(result.current.isStreaming).toBe(false);
    expect(result.current.error).toBe("The assistant failed.");
    expect(result.current.messages).toEqual([]);
  });

  it("WhenErrorEventReceivedAfterContent_ShouldKeepPartialReply", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "content", content: "Partial" },
        { $type: "error", message: "The assistant failed." },
      ),
    );
    const { result } = renderChat();

    // Act
    let sent = false;
    await act(async () => {
      sent = await result.current.send("Hi");
    });

    // Assert
    expect(sent).toBe(true);
    expect(result.current.error).toBe("The assistant failed.");
    expect(result.current.messages.map(({ content }) => content)).toEqual([
      "Hi",
      "Partial",
    ]);
  });

  it("WhenRequestThrows_ShouldSetError", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockImplementationOnce(async function* () {
      yield* [];
      throw new Error("Streaming failed: Internal Server Error");
    });
    const { result } = renderChat();

    // Act
    await act(() => result.current.send("Hi"));

    // Assert
    expect(result.current.error).toBe("Streaming failed: Internal Server Error");
    expect(result.current.messages).toEqual([]);
  });

  it("WhenStopped_ShouldAbortAndKeepPartialReply", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockImplementationOnce(async function* (
      _endpoint,
      _input,
      signal,
    ) {
      yield { $type: "content", content: "Partial" } as AssistantEvent;
      await new Promise((_, reject) =>
        signal?.addEventListener("abort", () =>
          reject(new DOMException("Aborted", "AbortError")),
        ),
      );
    });
    const { result } = renderChat();
    let pending: Promise<boolean> = Promise.resolve(true);
    act(() => {
      pending = result.current.send("Hi");
    });
    await waitFor(() => expect(result.current.messages.at(-1)?.content).toBe("Partial"));

    // Act
    await act(async () => {
      result.current.stop();
      await pending;
    });

    // Assert
    expect(result.current.isStreaming).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.messages.at(-1)?.content).toBe("Partial");
  });

  it("WhenMessageIsBlank_ShouldNotSend", async () => {
    // Arrange
    const { result } = renderChat();

    // Act
    await act(() => result.current.send("   "));

    // Assert
    expect(streamAssistant).not.toHaveBeenCalled();
    expect(result.current.messages).toEqual([]);
  });

  it("WhenResetMidStream_ShouldAllowImmediateSend", async () => {
    // Arrange
    vi.mocked(streamAssistant)
      .mockImplementationOnce(async function* (_endpoint, _input, signal) {
        yield { $type: "content", content: "Partial" } as AssistantEvent;
        await new Promise((_, reject) =>
          signal?.addEventListener("abort", () =>
            reject(new DOMException("Aborted", "AbortError")),
          ),
        );
      })
      .mockReturnValueOnce(events({ $type: "content", content: "Fresh" }));
    const { result } = renderChat();
    let pending: Promise<boolean> = Promise.resolve(true);
    act(() => {
      pending = result.current.send("Old");
    });
    await waitFor(() => expect(result.current.messages.at(-1)?.content).toBe("Partial"));

    // Act
    act(() => result.current.reset());
    await act(() => result.current.send("New"));
    await act(() => pending);

    // Assert
    expect(streamAssistant).toHaveBeenCalledTimes(2);
    expect(result.current.isStreaming).toBe(false);
    expect(result.current.messages.map(({ content }) => content)).toEqual([
      "New",
      "Fresh",
    ]);
  });
});
