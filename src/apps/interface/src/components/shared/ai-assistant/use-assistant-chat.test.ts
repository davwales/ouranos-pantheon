import { type AssistantChatActionPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import {
  useAssistantChat,
  type AssistantChatMessage,
} from "@/components/shared/ai-assistant/use-assistant-chat";
import {
  streamAssistant,
  type AssistantActionHandlers,
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

function renderChat(actions?: AssistantActionHandlers) {
  return renderHook(() =>
    useAssistantChat({ endpoint: ENDPOINT, context: CONTEXT, actions }),
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
    expect(result.current.messages[1].context).toEqual(CONTEXT);
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
      parts: [
        { kind: "reasoning", content: "Let me think", endedAt: expect.any(Number) },
        { kind: "text", content: "Answer" },
      ],
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

  it("WhenToolsAndActionsAreStreamed_ShouldRecordThemInOrderAndReportOutcomesInHistory", async () => {
    // Arrange
    vi.mocked(streamAssistant)
      .mockReturnValueOnce(
        events(
          { $type: "content", content: "Checking." },
          { $type: "tool", callId: "c1", name: "get_signals", arguments: '{"id":1}' },
          { $type: "toolResult", callId: "c1", status: "Completed", output: "[]", isTruncated: false, durationMs: 0 },
          { $type: "tool", callId: "c2", name: "add_note", arguments: '{"text":"Freezes well"}' },
          { $type: "action", callId: "c2", name: "add_note", action: { text: "Freezes well" } },
          { $type: "toolResult", callId: "c2", status: "Completed", output: "Proposed", isTruncated: false, durationMs: 0 },
          { $type: "content", content: "Proposed a note." },
          { $type: "done" },
        ),
      )
      .mockReturnValueOnce(events({ $type: "content", content: "Second" }));
    const { result } = renderChat({
      add_note: { describe: () => "Add note", apply: vi.fn() },
    });
    await act(() => result.current.send("One"));

    // Act
    await act(() => result.current.send("Two"));

    // Assert
    const reply = result.current.messages[1];
    expect(reply.content).toBe("Checking.\n\nProposed a note.");
    expect(reply.parts).toEqual([
      expect.objectContaining({ kind: "text", content: "Checking." }),
      expect.objectContaining({
        kind: "tool",
        name: "get_signals",
        arguments: '{"id":1}',
        status: "completed",
        output: "[]",
      }),
      expect.objectContaining({
        kind: "action",
        name: "add_note",
        action: { text: "Freezes well" },
        arguments: '{"text":"Freezes well"}',
        output: "Proposed",
        status: "proposed",
      }),
      expect.objectContaining({ kind: "text", content: "Proposed a note." }),
    ]);
    expect(vi.mocked(streamAssistant).mock.calls[1][1].messages).toEqual([
      { role: "User", content: "One" },
      {
        role: "Assistant",
        content: "Checking.\n\nProposed a note.",
        actions: [{ name: "add_note", description: "Add note", status: "Pending" }],
      },
      { role: "User", content: "Two" },
    ]);
  });

  it("WhenActionHasNoHandler_ShouldLeaveItOutOfTheHistory", async () => {
    // Arrange
    vi.mocked(streamAssistant)
      .mockReturnValueOnce(
        events(
          { $type: "action", callId: "c1", name: "unknown", action: {} },
          { $type: "content", content: "Proposed." },
        ),
      )
      .mockReturnValueOnce(events({ $type: "content", content: "Second" }));
    const { result } = renderChat();
    await act(() => result.current.send("One"));

    // Act
    await act(() => result.current.send("Two"));

    // Assert
    expect(vi.mocked(streamAssistant).mock.calls[1][1].messages[1]).toEqual({
      role: "Assistant",
      content: "Proposed.",
    });
  });

  it("WhenStoppedDuringATool_ShouldKeepTheTurnWithTheToolCancelledAndLeaveItOutOfTheHistory", async () => {
    // Arrange
    vi.mocked(streamAssistant)
      .mockImplementationOnce(async function* (_endpoint, _input, signal) {
        yield { $type: "tool", callId: "c1", name: "get_signals", arguments: "{}" } as AssistantEvent;
        await new Promise((_, reject) =>
          signal?.addEventListener("abort", () =>
            reject(new DOMException("Aborted", "AbortError")),
          ),
        );
      })
      .mockReturnValueOnce(events({ $type: "content", content: "Second" }));
    const { result } = renderChat();
    let pending: Promise<boolean> = Promise.resolve(true);
    act(() => {
      pending = result.current.send("One");
    });
    await waitFor(() => expect(result.current.messages[1]?.parts).toHaveLength(1));

    // Act
    await act(async () => {
      result.current.stop();
      await pending;
    });
    await act(() => result.current.send("Two"));

    // Assert
    expect(result.current.messages[1].parts).toEqual([
      expect.objectContaining({ kind: "tool", status: "cancelled" }),
    ]);
    expect(vi.mocked(streamAssistant).mock.calls[1][1].messages).toEqual([
      { role: "User", content: "One" },
      { role: "User", content: "Two" },
    ]);
  });

  it("WhenErrorFollowsAProposal_ShouldKeepTheTurnSoTheProposalIsNotLost", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "action", callId: "c1", name: "add_note", action: { text: "Rest it" } },
        { $type: "error", message: "The assistant failed." },
      ),
    );
    const { result } = renderChat({
      add_note: { describe: () => "Add note", apply: vi.fn() },
    });

    // Act
    let sent = false;
    await act(async () => {
      sent = await result.current.send("Add a note");
    });

    // Assert
    expect(sent).toBe(true);
    expect(result.current.error).toBe("The assistant failed.");
    expect(result.current.messages[1].parts).toEqual([
      expect.objectContaining({ kind: "action", status: "proposed" }),
    ]);
  });

  it("WhenErrorFollowsOnlyTools_ShouldWithdrawTheTurn", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "tool", callId: "c1", name: "get_signals", arguments: "{}" },
        { $type: "toolResult", callId: "c1", status: "Failed", output: "Boom", isTruncated: false, durationMs: 0 },
        { $type: "error", message: "The assistant failed." },
      ),
    );
    const { result } = renderChat();

    // Act
    let sent = true;
    await act(async () => {
      sent = await result.current.send("Hi");
    });

    // Assert
    expect(sent).toBe(false);
    expect(result.current.messages).toEqual([]);
  });

  it("WhenActionHandlerAutoApplies_ShouldApplyItAsItArrives", async () => {
    // Arrange
    const apply = vi.fn();
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "action", callId: "c1", name: "set_view", action: { sortBy: "Roi" } },
        { $type: "content", content: "Done." },
      ),
    );
    const { result } = renderChat({
      set_view: { describe: () => "Sort by ROI", apply, autoApply: true },
    });

    // Act
    await act(() => result.current.send("Sort by ROI"));

    // Assert
    expect(apply).toHaveBeenCalledWith({ sortBy: "Roi" });
    await waitFor(() => expect(actionsOf(result.current.messages[1])[0].status).toBe("applied"));
  });

  it("WhenProposedActionIsApplied_ShouldRunTheHandlerAndMarkItApplied", async () => {
    // Arrange
    const apply = vi.fn().mockResolvedValue(undefined);
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "action", callId: "c1", name: "add_note", action: { text: "Rest it" } },
        { $type: "content", content: "Proposed." },
      ),
    );
    const { result } = renderChat({
      add_note: { describe: () => "Add note", apply },
    });
    await act(() => result.current.send("Add a note"));
    const reply = result.current.messages[1];
    expect(apply).not.toHaveBeenCalled();

    // Act
    act(() => result.current.applyAction(reply.id, actionsOf(reply)[0].id));

    // Assert
    await waitFor(() => expect(actionsOf(result.current.messages[1])[0].status).toBe("applied"));
    expect(apply).toHaveBeenCalledWith({ text: "Rest it" });
  });

  it("WhenApplyingFails_ShouldMarkTheActionFailedWithTheError", async () => {
    // Arrange
    const apply = vi.fn().mockRejectedValue(new Error("Recipe changed"));
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "action", callId: "c1", name: "add_note", action: { text: "Rest it" } },
        { $type: "content", content: "Proposed." },
      ),
    );
    const { result } = renderChat({
      add_note: { describe: () => "Add note", apply },
    });
    await act(() => result.current.send("Add a note"));
    const reply = result.current.messages[1];

    // Act
    act(() => result.current.applyAction(reply.id, actionsOf(reply)[0].id));

    // Assert
    await waitFor(() =>
      expect(actionsOf(result.current.messages[1])[0]).toEqual(
        expect.objectContaining({ status: "failed", error: "Recipe changed" }),
      ),
    );
  });

  it("WhenProposedActionIsRejected_ShouldMarkItRejectedAndReportItInHistory", async () => {
    // Arrange
    const apply = vi.fn();
    vi.mocked(streamAssistant)
      .mockReturnValueOnce(
        events(
          { $type: "action", callId: "c1", name: "add_note", action: { text: "Rest it" } },
          { $type: "content", content: "Proposed." },
        ),
      )
      .mockReturnValueOnce(events({ $type: "content", content: "Ok." }));
    const { result } = renderChat({
      add_note: { describe: () => "Add note", apply },
    });
    await act(() => result.current.send("Add a note"));
    const reply = result.current.messages[1];

    // Act
    act(() => result.current.rejectAction(reply.id, actionsOf(reply)[0].id));
    act(() => result.current.applyAction(reply.id, actionsOf(reply)[0].id));
    await act(() => result.current.send("Why?"));

    // Assert
    expect(apply).not.toHaveBeenCalled();
    expect(actionsOf(result.current.messages[1])[0].status).toBe("rejected");
    expect(vi.mocked(streamAssistant).mock.calls[1][1].messages[1].actions).toEqual([
      { name: "add_note", description: "Add note", status: "Rejected" },
    ]);
  });

  it("WhenApplyingAll_ShouldApplyEveryProposalOneAfterAnother", async () => {
    // Arrange
    const order: string[] = [];
    const apply = vi.fn(async (action: unknown) => {
      const { text } = action as { text: string };
      order.push(`start ${text}`);
      await Promise.resolve();
      order.push(`end ${text}`);
    });
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "action", callId: "c1", name: "add_note", action: { text: "A" } },
        { $type: "action", callId: "c2", name: "add_note", action: { text: "B" } },
        { $type: "content", content: "Proposed." },
      ),
    );
    const { result } = renderChat({
      add_note: { describe: () => "Add note", apply },
    });
    await act(() => result.current.send("Add notes"));

    // Act
    await act(() => result.current.applyAll(result.current.messages[1].id));

    // Assert
    expect(order).toEqual(["start A", "end A", "start B", "end B"]);
    expect(actionsOf(result.current.messages[1]).map((a) => a.status)).toEqual([
      "applied",
      "applied",
    ]);
  });

  it("WhenRejectingAll_ShouldRejectOnlyTheOpenProposals", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events(
        { $type: "action", callId: "c1", name: "add_note", action: { text: "A" } },
        { $type: "action", callId: "c2", name: "add_note", action: { text: "B" } },
        { $type: "content", content: "Proposed." },
      ),
    );
    const { result } = renderChat({
      add_note: { describe: () => "Add note", apply: vi.fn() },
    });
    await act(() => result.current.send("Add notes"));
    const reply = result.current.messages[1];
    act(() => result.current.applyAction(reply.id, actionsOf(reply)[0].id));
    await waitFor(() => expect(actionsOf(result.current.messages[1])[0].status).toBe("applied"));

    // Act
    act(() => result.current.rejectAll(reply.id));

    // Assert
    expect(actionsOf(result.current.messages[1]).map((a) => a.status)).toEqual([
      "applied",
      "rejected",
    ]);
  });
});

function actionsOf(message: AssistantChatMessage): AssistantChatActionPart[] {
  return message.parts.filter((part) => part.kind === "action");
}
