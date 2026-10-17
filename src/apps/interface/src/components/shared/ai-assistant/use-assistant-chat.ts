"use client";

import {
  actionOutcome,
  applyEvent,
  finishParts,
  textOf,
  type AssistantChatActionPart,
  type AssistantChatPart,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import {
  streamAssistant,
  type AssistantActionHandlers,
  type AssistantEndpoint,
  type AssistantMessageInput,
  type AssistantRole,
} from "@/lib/api/assistant";
import { generateID } from "@/lib/utils";
import { useCallback, useEffect, useRef, useState } from "react";

export type AssistantChatMessage = {
  id: string;
  role: AssistantRole;
  content: string;
  parts: AssistantChatPart[];
  context?: unknown;
};

export type UseAssistantChatOptions<TContext> = {
  endpoint: AssistantEndpoint<TContext>;
  context: TContext;
  actions?: AssistantActionHandlers;
};

const FALLBACK_ERROR = "The assistant is unavailable right now. Please try again.";
const FALLBACK_ACTION_ERROR = "The change could not be applied.";

export function useAssistantChat<TContext>({
  endpoint,
  context,
  actions,
}: UseAssistantChatOptions<TContext>) {
  const [messages, setMessages] = useState<AssistantChatMessage[]>([]);
  const [isStreaming, setIsStreaming] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);
  const actionsRef = useRef(actions);

  useEffect(() => {
    actionsRef.current = actions;
  }, [actions]);

  useEffect(() => {
    return () => abortRef.current?.abort();
  }, []);

  const updateParts = useCallback(
    (
      messageId: string,
      update: (parts: AssistantChatPart[]) => AssistantChatPart[],
    ) => {
      setMessages((prev) =>
        prev.map((m) => {
          if (m.id !== messageId) {
            return m;
          }
          const parts = update(m.parts);
          return { ...m, parts, content: textOf(parts) };
        }),
      );
    },
    [],
  );

  const updateAction = useCallback(
    (
      messageId: string,
      actionId: string,
      update: Partial<AssistantChatActionPart>,
    ) => {
      updateParts(messageId, (parts) =>
        parts.map((part) =>
          part.kind === "action" && part.id === actionId
            ? { ...part, ...update }
            : part,
        ),
      );
    },
    [updateParts],
  );

  const runAction = useCallback(
    async (messageId: string, chatAction: AssistantChatActionPart) => {
      const handler = actionsRef.current?.[chatAction.name];
      if (!handler) {
        return;
      }

      updateAction(messageId, chatAction.id, { status: "applying", error: undefined });
      try {
        await handler.apply(chatAction.action);
        updateAction(messageId, chatAction.id, { status: "applied" });
      } catch (e) {
        updateAction(messageId, chatAction.id, {
          status: "failed",
          error: e instanceof Error && e.message ? e.message : FALLBACK_ACTION_ERROR,
        });
      }
    },
    [updateAction],
  );

  const findActions = useCallback(
    (messageId: string) => {
      return (
        messages
          .find((m) => m.id === messageId)
          ?.parts.filter((part) => part.kind === "action") ?? []
      );
    },
    [messages],
  );

  const applyAction = useCallback(
    (messageId: string, actionId: string) => {
      const chatAction = findActions(messageId).find((a) => a.id === actionId);
      if (chatAction && (chatAction.status === "proposed" || chatAction.status === "failed")) {
        void runAction(messageId, chatAction);
      }
    },
    [findActions, runAction],
  );

  const rejectAction = useCallback(
    (messageId: string, actionId: string) => {
      updateParts(messageId, (parts) =>
        parts.map((part) =>
          part.kind === "action" &&
          part.id === actionId &&
          (part.status === "proposed" || part.status === "failed")
            ? { ...part, status: "rejected", error: undefined }
            : part,
        ),
      );
    },
    [updateParts],
  );

  // Applied one after another because handlers such as recipe edits save the latest state, and
  // concurrent saves would overwrite each other.
  const applyAll = useCallback(
    async (messageId: string) => {
      const proposed = findActions(messageId).filter((a) => a.status === "proposed");
      for (const chatAction of proposed) {
        await runAction(messageId, chatAction);
      }
    },
    [findActions, runAction],
  );

  const rejectAll = useCallback(
    (messageId: string) => {
      updateParts(messageId, (parts) =>
        parts.map((part) =>
          part.kind === "action" && part.status === "proposed"
            ? { ...part, status: "rejected" }
            : part,
        ),
      );
    },
    [updateParts],
  );

  const send = useCallback(
    async (content: string): Promise<boolean> => {
      const text = content.trim();
      if (!text || abortRef.current) {
        return false;
      }

      const userId = generateID();
      const history: AssistantChatMessage[] = [
        ...messages,
        { id: userId, role: "User", content: text, parts: [] },
      ];
      const assistantId = generateID();
      const controller = new AbortController();
      abortRef.current = controller;

      setMessages([
        ...history,
        { id: assistantId, role: "Assistant", content: "", parts: [], context },
      ]);
      setIsStreaming(true);
      setError(null);

      let hasText = false;
      let hasAction = false;
      let failure: string | null = null;
      try {
        const stream = streamAssistant(
          endpoint,
          {
            messages: toHistory(history, actionsRef.current),
            context,
          },
          controller.signal,
        );
        for await (const event of stream) {
          if (event.$type === "error") {
            failure = event.message;
            break;
          }
          if (
            event.$type === "usage" ||
            event.$type === "done" ||
            event.$type === "instructions"
          ) {
            continue;
          }

          const id = generateID();
          const now = Date.now();
          hasText ||= event.$type === "content";
          hasAction ||= event.$type === "action";
          updateParts(assistantId, (parts) => applyEvent(parts, event, now, id));

          if (event.$type === "action" && actionsRef.current?.[event.name]?.autoApply) {
            void runAction(assistantId, {
              kind: "action",
              id,
              callId: event.callId,
              name: event.name,
              action: event.action,
              status: "proposed",
              arguments: "",
            });
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

      // A failed turn with nothing to keep is withdrawn so the user can resend it without
      // leaving two consecutive user messages in the history. A proposal is never thrown away.
      const isWithdrawn = failure !== null && !hasText && !hasAction;
      const now = Date.now();
      setMessages((prev) =>
        prev.flatMap((m) => {
          if (m.id === userId) {
            return isWithdrawn ? [] : [m];
          }
          if (m.id !== assistantId) {
            return [m];
          }
          if (isWithdrawn || m.parts.length === 0) {
            return [];
          }
          return [{ ...m, parts: finishParts(m.parts, now) }];
        }),
      );
      setError(failure);
      setIsStreaming(false);
      return !isWithdrawn;
    },
    [endpoint, context, messages, runAction, updateParts],
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

  return {
    messages,
    isStreaming,
    error,
    send,
    stop,
    reset,
    applyAction,
    rejectAction,
    applyAll,
    rejectAll,
  };
}

function toHistory(
  messages: AssistantChatMessage[],
  handlers: AssistantActionHandlers | undefined,
): AssistantMessageInput[] {
  return messages.flatMap(({ role, content, parts }) => {
    const outcomes = parts.flatMap((part) => {
      const handler = part.kind === "action" ? handlers?.[part.name] : undefined;
      return part.kind === "action" && handler
        ? [
            {
              name: part.name,
              description: handler.describe(part.action),
              status: actionOutcome(part.status),
            },
          ]
        : [];
    });

    if (outcomes.length > 0) {
      return [{ role, content, actions: outcomes }];
    }
    // A reply stopped before it said anything carries nothing the model can use.
    return content.length > 0 || role === "User" ? [{ role, content }] : [];
  });
}
