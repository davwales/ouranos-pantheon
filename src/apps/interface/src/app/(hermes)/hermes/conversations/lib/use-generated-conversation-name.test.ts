import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { hermesApi, type SavedConversation } from "@/lib/api/hermes";

import {
  DEFAULT_CONVERSATION_NAME,
  NAME_POLL_INTERVAL_MS,
  NAME_POLL_WINDOW_MS,
  useGeneratedConversationName,
} from "./use-generated-conversation-name";

const NOW = Date.parse("2026-10-31T12:00:00Z");

function savedWithName(name: string): SavedConversation {
  return { name } as SavedConversation;
}

describe("useGeneratedConversationName", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(NOW);
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it("picks up the name generated after a fresh save", async () => {
    const getConversation = vi
      .spyOn(hermesApi, "getConversation")
      .mockResolvedValueOnce(savedWithName(DEFAULT_CONVERSATION_NAME))
      .mockResolvedValueOnce(savedWithName("Trip planning"));

    const { result } = renderHook(() =>
      useGeneratedConversationName("c1", DEFAULT_CONVERSATION_NAME, new Date(NOW).toISOString()),
    );

    await act(() => vi.advanceTimersByTimeAsync(NAME_POLL_INTERVAL_MS));
    expect(result.current).toBeUndefined();

    await act(() => vi.advanceTimersByTimeAsync(NAME_POLL_INTERVAL_MS));
    expect(result.current).toBe("Trip planning");
    expect(getConversation).toHaveBeenCalledTimes(2);

    await act(() => vi.advanceTimersByTimeAsync(NAME_POLL_INTERVAL_MS * 3));
    expect(getConversation).toHaveBeenCalledTimes(2);
  });

  it("does not poll once the conversation has a name", async () => {
    const getConversation = vi.spyOn(hermesApi, "getConversation");

    renderHook(() =>
      useGeneratedConversationName("c1", "My chat", new Date(NOW).toISOString()),
    );
    await act(() => vi.advanceTimersByTimeAsync(NAME_POLL_INTERVAL_MS * 3));

    expect(getConversation).not.toHaveBeenCalled();
  });

  it("does not poll an old conversation that was never named", async () => {
    const getConversation = vi.spyOn(hermesApi, "getConversation");
    const createdAt = new Date(NOW - NAME_POLL_WINDOW_MS - 1).toISOString();

    renderHook(() =>
      useGeneratedConversationName("c1", DEFAULT_CONVERSATION_NAME, createdAt),
    );
    await act(() => vi.advanceTimersByTimeAsync(NAME_POLL_INTERVAL_MS * 3));

    expect(getConversation).not.toHaveBeenCalled();
  });

  it("stops polling when the window ends without a name", async () => {
    const getConversation = vi
      .spyOn(hermesApi, "getConversation")
      .mockResolvedValue(savedWithName(DEFAULT_CONVERSATION_NAME));

    const { result } = renderHook(() =>
      useGeneratedConversationName("c1", DEFAULT_CONVERSATION_NAME, new Date(NOW).toISOString()),
    );
    await act(() => vi.advanceTimersByTimeAsync(NAME_POLL_WINDOW_MS * 2));

    expect(result.current).toBeUndefined();
    expect(getConversation.mock.calls.length).toBeLessThanOrEqual(
      NAME_POLL_WINDOW_MS / NAME_POLL_INTERVAL_MS,
    );
  });
});
