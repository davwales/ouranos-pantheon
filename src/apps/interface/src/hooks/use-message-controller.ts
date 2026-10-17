"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useStreamingMessage } from "@/hooks/use-streaming-message";
import { streamCompletion, streamCompact } from "@/lib/api/hermes";
import {
  type MessageInput,
  type ReasoningEffort,
  Role,
} from "@/lib/api/hermes-types";
import { type LoraFormInput, TraitFormInput } from "@/app/(hermes)/hermes/types";
import {
  type AssistantChatPart,
  applyEvent,
  finishParts,
  textOf,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import {
  toMessageParts,
  toolCallsOf,
  withEditedText,
} from "@/app/(hermes)/hermes/components/message-parts";
import { generateID } from "@/lib/utils";

const FALLBACK_ERROR = "The response could not be generated. Please try again.";

export type TokenUsage = {
  inputTokens: number;
  outputTokens: number;
  totalTokens: number;
};

export function useMessageController(params: {
  initialMessages: MessageInput[];
  initialTokenUsage: TokenUsage | null;
  model: {
    modelIdentifier: string;
    systemPrompt: string;
    temperature?: number | null;
    maxTokens?: number | null;
    isToolCallingEnabled: boolean;
  };
  reasoningEffort: ReasoningEffort | null;
  persona: {
    name: string;
    description: string;
    personality?: string | null;
    scenario?: string | null;
  };
  activeTraits: TraitFormInput[];
  activeLoras: LoraFormInput[];
  conversationId?: string;
  onTokenUsageChange?: (usage: TokenUsage | null) => void;
  onSystemPromptChange?: (prompt: string | null) => void;
  onMessagesPersist?: (messages: MessageInput[]) => void;
}): {
  messages: MessageInput[];
  streamingParts: AssistantChatPart[] | null;
  isGenerating: boolean;
  generationError: string | null;
  stop: () => void;
  isCompacting: boolean;
  compactionError: string | null;
  tokenUsage: TokenUsage | null;
  addUserMessage: (content: string) => Promise<void>;
  updateMessageContent: (index: number, content: string) => void;
  deleteMessage: (index: number) => void;
  retryMessage: (index: number) => Promise<void>;
  compactMessages: (messagesToCompact: MessageInput[]) => Promise<void>;
  setMessages: React.Dispatch<React.SetStateAction<MessageInput[]>>;
} {
  const {
    initialMessages,
    initialTokenUsage,
    model,
    reasoningEffort,
    persona,
    activeTraits,
    activeLoras,
    conversationId,
    onTokenUsageChange,
    onSystemPromptChange,
    onMessagesPersist,
  } = params;

  const [messages, setMessages] = useState<MessageInput[]>(initialMessages);
  const [isCompacting, setIsCompacting] = useState(false);
  const [compactionError, setCompactionError] = useState<string | null>(null);
  const [tokenUsage, setTokenUsage] = useState<TokenUsage | null>(
    initialTokenUsage,
  );

  const [streamingParts, setStreamingParts] = useState<AssistantChatPart[] | null>(null);
  const [generationError, setGenerationError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const { isStreaming: isCompactionStreaming, startStream } = useStreamingMessage();
  const isGenerating = streamingParts !== null || isCompactionStreaming;

  useEffect(() => () => abortRef.current?.abort(), []);

  const getMessagesForLlm = useCallback(
    (allMessages: MessageInput[]): MessageInput[] => {
      const lastSummaryIndex = allMessages.findLastIndex(
        (m) => m.role === Role.Summary,
      );

      if (lastSummaryIndex === -1) {
        return allMessages;
      }

      const messagesAfterSummary = allMessages.slice(lastSummaryIndex);
      return messagesAfterSummary.map((m) =>
        m.role === Role.Summary ? { ...m, role: Role.Assistant } : m,
      );
    },
    [],
  );

  const generateCompletion = useCallback(
    async (currentMessages: MessageInput[]) => {
      if (isGenerating) return;

      const controller = new AbortController();
      abortRef.current = controller;
      setGenerationError(null);
      setStreamingParts([]);
      setMessages((prev) => [...prev, { role: Role.Assistant, content: "" }]);

      let parts: AssistantChatPart[] = [];
      let failure: string | null = null;
      try {
        const stream = streamCompletion(
          {
            conversation: {
              model: {
                modelIdentifier: model.modelIdentifier,
                systemPrompt: model.systemPrompt,
                temperature: model.temperature,
                maxTokens: model.maxTokens,
                isToolCallingEnabled: model.isToolCallingEnabled,
              },
              persona: {
                name: persona.name,
                description: persona.description,
                personality: persona.personality,
                scenario: persona.scenario,
              },
              traits: activeTraits.map((t) => ({
                name: t.name,
                content: t.content,
              })),
              messages: getMessagesForLlm(currentMessages).map(
                ({ role, content, parts: messageParts }) => ({
                  role,
                  content,
                  toolCalls: toolCallsOf(messageParts),
                }),
              ),
              reasoningEffort,
              loras: activeLoras.map((l) => ({
                identifier: l.identifier,
                weight: l.weight,
              })),
            },
            ...(conversationId ? { conversationId } : {}),
          },
          controller.signal,
        );

        for await (const event of stream) {
          switch (event.$type) {
            case "instructions":
              onSystemPromptChange?.(event.instructions);
              break;
            case "usage": {
              const usage = {
                inputTokens: event.inputTokens,
                outputTokens: event.outputTokens,
                totalTokens: event.contextTokens,
              };
              setTokenUsage(usage);
              onTokenUsageChange?.(usage);
              break;
            }
            case "error":
              failure = event.message;
              break;
            case "done":
            case "action":
              break;
            default:
              parts = applyEvent(parts, event, Date.now(), generateID());
              setStreamingParts(parts);
          }
        }
      } catch (error) {
        if (!controller.signal.aborted) {
          failure = error instanceof Error && error.message ? error.message : FALLBACK_ERROR;
        }
      }

      if (abortRef.current === controller) {
        abortRef.current = null;
      }

      // A reply with neither text nor an image is dropped so the user can retry from their own
      // message.
      const finished = finishParts(parts, Date.now());
      const content = textOf(finished);
      const isAnswered = content !== "" || finished.some((part) => part.kind === "image");

      setMessages((prev) => {
        const updated = prev.slice(0, -1);
        return isAnswered
          ? [...updated, { role: Role.Assistant, content, parts: toMessageParts(finished) }]
          : updated;
      });

      setGenerationError(failure);
      setStreamingParts(null);
    },
    [
      isGenerating,
      getMessagesForLlm,
      model,
      reasoningEffort,
      persona,
      activeTraits,
      activeLoras,
      conversationId,
      onTokenUsageChange,
      onSystemPromptChange,
    ],
  );

  const stop = useCallback(() => {
    abortRef.current?.abort();
  }, []);

  const addUserMessage = useCallback(
    async (content: string) => {
      if (!content.trim() || isCompacting || isGenerating) return;

      const userMessage: MessageInput = { role: Role.User, content };
      const updatedMessages = [...messages, userMessage];
      setMessages(updatedMessages);
      await generateCompletion(updatedMessages);
    },
    [messages, isCompacting, isGenerating, generateCompletion],
  );

  const updateMessageContent = useCallback(
    (index: number, content: string) => {
      setMessages((prev) => {
        const updatedMessages = [...prev];
        const message = updatedMessages[index];
        updatedMessages[index] = {
          ...message,
          content,
          parts: withEditedText(message.parts, content),
        };
        if (conversationId) {
          onMessagesPersist?.(updatedMessages);
        }
        return updatedMessages;
      });
    },
    [conversationId, onMessagesPersist],
  );

  const deleteMessage = useCallback(
    (index: number) => {
      setMessages((prev) => {
        const updatedMessages =
          prev[index].role === Role.Summary
            ? prev.filter((_, i) => i !== index)
            : prev.filter((_, i) => i < index);
        if (conversationId) {
          onMessagesPersist?.(updatedMessages);
        }
        return updatedMessages;
      });
    },
    [conversationId, onMessagesPersist],
  );

  const compactMessages = useCallback(async (messagesToCompact: MessageInput[]) => {
    if (isCompacting || isGenerating) return;

    const compactableMessages = messagesToCompact.filter(
      (m) => m.role === Role.User || m.role === Role.Assistant,
    );
    if (compactableMessages.length === 0) return;

    setIsCompacting(true);
    setCompactionError(null);

    const summaryMessage: MessageInput = {
      role: Role.Summary,
      content: "",
    };
    setMessages((prev) => [...prev, summaryMessage]);

    await startStream({
      generator: () =>
        streamCompact({
          ...(conversationId ? { conversationId } : {}),
          modelIdentifier: model.modelIdentifier,
          systemPrompt: model.systemPrompt,
          personaName: persona.name,
          personaDescription: persona.description,
          messages: messagesToCompact,
        }),
      chunkHandlers: {
        onContent: (content) => {
          setMessages((prev) => {
            const updated = [...prev];
            updated[updated.length - 1] = {
              ...updated[updated.length - 1],
              content: updated[updated.length - 1].content + content,
            };
            return updated;
          });
        },
        onUsage: (usage) => {
          const compactUsage = {
            inputTokens: 0,
            outputTokens: usage.outputTokens,
            totalTokens: usage.outputTokens,
          };
          setTokenUsage(compactUsage);
          onTokenUsageChange?.(compactUsage);
        },
        onComplete: () => {
          setIsCompacting(false);
        },
      },
      onError: (error) => {
        console.error("Error compacting conversation:", error);
        setCompactionError("Compaction failed. Please try again.");
        setIsCompacting(false);
      },
    });
  }, [
    isCompacting,
    isGenerating,
    startStream,
    model,
    persona,
    conversationId,
    onTokenUsageChange,
  ]);

  const retryMessage = useCallback(
    async (index: number) => {
      const updatedMessages = messages.filter((_, i) => i < index);
      setMessages(updatedMessages);

      if (messages[index].role === Role.Summary) {
        await compactMessages(updatedMessages);
        return;
      }

      await generateCompletion(updatedMessages);
    },
    [messages, compactMessages, generateCompletion],
  );

  return {
    messages,
    streamingParts,
    isGenerating,
    generationError,
    stop,
    isCompacting,
    compactionError,
    tokenUsage,
    addUserMessage,
    updateMessageContent,
    deleteMessage,
    retryMessage,
    compactMessages,
    setMessages,
  };
}
