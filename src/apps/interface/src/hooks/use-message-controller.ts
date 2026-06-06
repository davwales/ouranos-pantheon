"use client";

import { useCallback, useState } from "react";
import { useStreamingMessage } from "@/hooks/use-streaming-message";
import { streamCompletion, streamCompact } from "@/lib/api/hermes";
import { MessageInput, Role } from "@/lib/api/hermes-types";
import { TraitFormInput } from "@/app/(hermes)/hermes/types";

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
    repeatPenalty?: number | null;
  };
  persona: {
    name: string;
    description: string;
    personality?: string | null;
    scenario?: string | null;
  };
  activeTraits: TraitFormInput[];
  conversationId?: string;
  onTokenUsageChange?: (usage: TokenUsage | null) => void;
  onSystemPromptChange?: (prompt: string | null) => void;
  onMessagesPersist?: (messages: MessageInput[]) => void;
}): {
  messages: MessageInput[];
  isGenerating: boolean;
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
    persona,
    activeTraits,
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

  const { isStreaming: isGenerating, startStream } = useStreamingMessage();

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

      const assistantMessage: MessageInput = {
        role: Role.Assistant,
        content: "",
      };
      setMessages((prev) => [...prev, assistantMessage]);

      const messagesForLlm = getMessagesForLlm(currentMessages);

      await startStream({
        generator: () =>
          streamCompletion({
            conversation: {
              model: {
                modelIdentifier: model.modelIdentifier,
                systemPrompt: model.systemPrompt,
                temperature: model.temperature,
                maxTokens: model.maxTokens,
                repeatPenalty: model.repeatPenalty,
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
              messages: messagesForLlm.map(({ role, content }) => ({
                role,
                content,
              })),
            },
            ...(conversationId ? { conversationId } : {}),
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
            setTokenUsage(usage);
            onTokenUsageChange?.(usage);
          },
          onSystemPrompt: (systemPrompt) => {
            onSystemPromptChange?.(systemPrompt);
          },
        },
        onError: (error) => {
          console.error("Error sending message:", error);
        },
      });
    },
    [
      isGenerating,
      startStream,
      getMessagesForLlm,
      model,
      persona,
      activeTraits,
      conversationId,
      onTokenUsageChange,
      onSystemPromptChange,
    ],
  );

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
        updatedMessages[index] = { ...updatedMessages[index], content };
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
    isGenerating,
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
