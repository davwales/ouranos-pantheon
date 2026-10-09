"use client";

import {
  streamAssistant,
  type AssistantEndpoint,
  type AssistantRole,
} from "@/lib/api/assistant";
import { generateID } from "@/lib/utils";
import { useCallback, useEffect, useRef, useState } from "react";

export type AssistantChatMessage = {
  id: string;
  role: AssistantRole;
  content: string;
  reasoning?: string;
};

export type UseAssistantChatOptions<TContext> = {
  endpoint: AssistantEndpoint<TContext>;
  context: TContext;
};

const FALLBACK_ERROR = "The assistant is unavailable right now. Please try again.";

export function useAssistantChat<TContext>({
  endpoint,
  context,
}: UseAssistantChatOptions<TContext>) {
  const [messages, setMessages] = useState<AssistantChatMessage[]>([]);
  const [isStreaming, setIsStreaming] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    return () => abortRef.current?.abort();
  }, []);

  const send = useCallback(
    async (content: string): Promise<boolean> => {
      const text = content.trim();
      if (!text || abortRef.current) {
        return false;
      }

      const userId = generateID();
      const history: AssistantChatMessage[] = [
        ...messages,
        { id: userId, role: "User", content: text },
      ];
      const assistantId = generateID();
      const controller = new AbortController();
      abortRef.current = controller;

      setMessages([
        ...history,
        { id: assistantId, role: "Assistant", content: "" },
      ]);
      setIsStreaming(true);
      setError(null);

      let reply = "";
      let reasoning = "";
      let failure: string | null = null;
      try {
        const stream = streamAssistant(
          endpoint,
          {
            messages: history.map(({ role, content }) => ({ role, content })),
            context,
          },
          controller.signal,
        );
        for await (const event of stream) {
          if (event.$type === "reasoning") {
            reasoning += event.content;
            const snapshot = reasoning;
            setMessages((prev) =>
              prev.map((m) =>
                m.id === assistantId ? { ...m, reasoning: snapshot } : m,
              ),
            );
          } else if (event.$type === "content") {
            reply += event.content;
            const snapshot = reply;
            setMessages((prev) =>
              prev.map((m) =>
                m.id === assistantId ? { ...m, content: snapshot } : m,
              ),
            );
          } else if (event.$type === "error") {
            failure = event.message;
            break;
          }
        }
      } catch (e) {
        if (!controller.signal.aborted) {
          failure = e instanceof Error && e.message ? e.message : FALLBACK_ERROR;
        }
      }

      if (abortRef.current !== controller) {
        return true;
      }
      abortRef.current = null;

      // A failed turn with no reply is withdrawn so the user can resend it without
      // leaving two consecutive user messages in the history.
      const isWithdrawn = failure !== null && reply.length === 0;
      setMessages((prev) =>
        prev.filter(
          (m) =>
            (m.id !== assistantId || m.content.length > 0) &&
            (m.id !== userId || !isWithdrawn),
        ),
      );
      setError(failure);
      setIsStreaming(false);
      return !isWithdrawn;
    },
    [endpoint, context, messages],
  );

  const stop = useCallback(() => {
    abortRef.current?.abort();
  }, []);

  const reset = useCallback(() => {
    abortRef.current?.abort();
    abortRef.current = null;
    setMessages([]);
    setError(null);
    setIsStreaming(false);
  }, []);

  return { messages, isStreaming, error, send, stop, reset };
}
