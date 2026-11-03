"use client";

import { useEffect, useState } from "react";

import { hermesApi } from "@/lib/api/hermes";

export const DEFAULT_CONVERSATION_NAME = "New Conversation";
export const NAME_POLL_INTERVAL_MS = 2_000;
export const NAME_POLL_WINDOW_MS = 60_000;

/**
 * The gateway names a new conversation in the background after saving it, so a freshly saved
 * conversation still carries the default name; poll briefly until the generated one lands.
 */
export function useGeneratedConversationName(
  conversationId: string,
  name: string | undefined,
  createdAt: string | undefined,
): string | undefined {
  const [generatedName, setGeneratedName] = useState<string>();

  useEffect(() => {
    if (name !== DEFAULT_CONVERSATION_NAME || !createdAt) return;

    const deadline = Date.parse(createdAt) + NAME_POLL_WINDOW_MS;
    if (Number.isNaN(deadline) || Date.now() >= deadline) return;

    let cancelled = false;
    let timer: ReturnType<typeof setTimeout>;

    const poll = async () => {
      try {
        const conversation = await hermesApi.getConversation(conversationId);
        if (cancelled) return;
        if (conversation.name !== DEFAULT_CONVERSATION_NAME) {
          setGeneratedName(conversation.name);
          return;
        }
      } catch {
        if (cancelled) return;
      }
      if (Date.now() < deadline) {
        timer = setTimeout(poll, NAME_POLL_INTERVAL_MS);
      }
    };

    timer = setTimeout(poll, NAME_POLL_INTERVAL_MS);
    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [conversationId, name, createdAt]);

  return name === DEFAULT_CONVERSATION_NAME ? generatedName : undefined;
}
