"use client";

import { FooterContent } from "@/app/components/footer";
import { useNavBarActions } from "@/app/components/nav-bar-actions-context";
import ChatInput from "@/app/hermes/components/chat_input";
import ChatMessageList from "@/app/hermes/components/chat_message_list";
import { ContextUsageBar } from "@/app/hermes/components/context-usage-bar";
import { ConversationConfigSheet } from "@/app/hermes/components/conversation-config-sheet";
import {
  ModelFormInput,
  PersonaFormInput,
  TraitFormInput,
} from "@/app/hermes/types";
import {
  hermesApi,
  MessageInput,
  Role,
  streamCompact,
  streamCompletion,
} from "@/lib/api/hermes";
import { Bookmark, SlidersHorizontal } from "lucide-react";
import { useCallback, useEffect, useState } from "react";

export default function ChatInterfaceView({
  persona,
  model,
  activeTraits,
  onPersonaChange,
  onModelChange,
  onTraitsChange,
  conversationId,
  conversationName,
  conversationIsPublic = true,
  initialMessages,
  initialTokenUsage,
  onConversationSaved,
  onRename,
  onDelete,
  onVisibilityChange,
  ...props
}: React.ComponentProps<"div"> & {
  persona: PersonaFormInput;
  model: ModelFormInput;
  activeTraits: TraitFormInput[];
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
  onTraitsChange?: (traits: TraitFormInput[]) => void;
  conversationId?: string;
  conversationName?: string;
  conversationIsPublic?: boolean;
  initialMessages?: MessageInput[];
  initialTokenUsage?: {
    inputTokens: number;
    outputTokens: number;
    totalTokens: number;
  } | null;
  onConversationSaved?: (id: string, name: string) => void;
  onRename?: (name: string) => void;
  onDelete?: () => void;
  onVisibilityChange?: (isPublic: boolean) => void;
}) {
  const [messages, setMessages] = useState<MessageInput[]>(
    initialMessages ?? [],
  );
  const [inputText, setInputText] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [isCompacting, setIsCompacting] = useState(false);
  const [tokenUsage, setTokenUsage] = useState<{
    inputTokens: number;
    outputTokens: number;
    totalTokens: number;
  } | null>(initialTokenUsage ?? null);
  const [editingMessageIndex, setEditingMessageIndex] = useState<number | null>(
    null,
  );
  const [isConfigOpen, setIsConfigOpen] = useState(false);
  const { setActions, clearActions } = useNavBarActions();

  const buildUpdatePayload = useCallback(
    (overrides: {
      messages?: MessageInput[];
      personaId?: string;
      modelConfigId?: string;
      traitIds?: string[];
    }) => ({
      name: conversationName ?? "New Conversation",
      personaId: overrides.personaId ?? persona.id!,
      modelConfigId: overrides.modelConfigId ?? model.id!,
      traitIds:
        overrides.traitIds ??
        activeTraits.filter((t) => t.id && !t.isEphemeral).map((t) => t.id!),
      messages: overrides.messages ?? messages,
      isPublic: conversationIsPublic,
    }),
    [
      conversationName,
      conversationIsPublic,
      persona.id,
      model.id,
      activeTraits,
      messages,
    ],
  );

  const handleSaveConversation = useCallback(async () => {
    try {
      const result = await hermesApi.createConversation({
        personaId: persona.id!,
        modelConfigId: model.id!,
        traitIds: activeTraits
          .filter((t) => t.id && !t.isEphemeral)
          .map((t) => t.id!),
        messages: messages,
        inputTokenCount: tokenUsage?.inputTokens ?? null,
        outputTokenCount: tokenUsage?.outputTokens ?? null,
        totalTokenCount: tokenUsage?.totalTokens ?? null,
      });
      onConversationSaved?.(result.id, result.name);
    } catch (error) {
      console.error("Error saving conversation:", error);
    }
  }, [
    persona.id,
    model.id,
    activeTraits,
    messages,
    tokenUsage,
    onConversationSaved,
  ]);

  useEffect(() => {
    const actions = (
      <div className="flex items-center gap-1">
        {!conversationId && messages.length > 0 && !isGenerating && (
          <button
            onClick={handleSaveConversation}
            className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors"
            aria-label="Save conversation"
          >
            <Bookmark className="h-5 w-5" />
          </button>
        )}
        <button
          onClick={() => setIsConfigOpen(true)}
          disabled={isGenerating}
          className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          aria-label="Configure conversation"
        >
          <SlidersHorizontal className="h-5 w-5" />
        </button>
      </div>
    );
    setActions(actions);
    return () => clearActions();
  }, [
    setActions,
    clearActions,
    handleSaveConversation,
    messages.length,
    conversationId,
    isGenerating,
  ]);

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

  const generateCompletion = async (currentMessages: MessageInput[]) => {
    if (isGenerating) return;
    setIsGenerating(true);

    const assistantMessage: MessageInput = {
      role: Role.Assistant,
      content: "",
    };
    setMessages((prev) => [...prev, assistantMessage]);

    const messagesForLlm = getMessagesForLlm(currentMessages);

    try {
      for await (const chunk of streamCompletion({
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
      })) {
        if (chunk.$type === "usage") {
          setTokenUsage({
            inputTokens: chunk.inputTokens,
            outputTokens: chunk.outputTokens,
            totalTokens: chunk.totalTokens,
          });
        } else if (chunk.$type === "content") {
          setMessages((prev) => {
            const updated = [...prev];
            updated[updated.length - 1] = {
              ...updated[updated.length - 1],
              content: updated[updated.length - 1].content + chunk.content,
            };
            return updated;
          });
        }
      }
    } catch (error) {
      console.error("Error sending message:", error);
    } finally {
      setIsGenerating(false);
    }
  };

  const handleUpdateMessage = () => {
    if (!editingMessageIndex || !inputText.trim()) return;
    setMessages((prev) => {
      const updatedMessages = [...prev];
      updatedMessages[editingMessageIndex].content = inputText;
      if (conversationId) {
        hermesApi
          .updateConversation(
            conversationId,
            buildUpdatePayload({ messages: updatedMessages }),
          )
          .catch((err) => console.error("Failed to update conversation:", err));
      }
      return updatedMessages;
    });
    setEditingMessageIndex(null);
    setInputText("");
  };

  const handleNewMessage = async () => {
    if (!inputText.trim()) return;
    const userMessage: MessageInput = { role: Role.User, content: inputText };
    const updatedMessages = [...messages, userMessage];
    setMessages(updatedMessages);
    setInputText("");
    await generateCompletion(updatedMessages);
  };

  const handleMessageEdit = (index: number) => {
    setInputText(messages[index].content);
    setEditingMessageIndex(index);
  };

  const handleCancelEdit = () => {
    setEditingMessageIndex(null);
    setInputText("");
  };

  const handleMessageDeleted = (index: number) => {
    setMessages((prev) => {
      const updatedMessages = prev.filter((_, i) => i < index);
      if (conversationId) {
        hermesApi
          .updateConversation(
            conversationId,
            buildUpdatePayload({ messages: updatedMessages }),
          )
          .catch((err) => console.error("Failed to update conversation:", err));
      }
      return updatedMessages;
    });
  };

  const handleMessageRetry = async (index: number) => {
    const updatedMessages = messages.filter((_, i) => i < index);
    setMessages(updatedMessages);
    await generateCompletion(updatedMessages);
  };

  const handleCompact = async () => {
    if (isCompacting || isGenerating) return;

    const compactableMessages = messages.filter(
      (m) => m.role === Role.User || m.role === Role.Assistant,
    );
    if (compactableMessages.length === 0) return;

    setIsCompacting(true);
    try {
      const summaryMessage: MessageInput = {
        role: Role.Summary,
        content: "",
      };
      setMessages((prev) => [...prev, summaryMessage]);

      for await (const chunk of streamCompact({
        ...(conversationId ? { conversationId } : {}),
        modelIdentifier: model.modelIdentifier,
        systemPrompt: model.systemPrompt,
        personaName: persona.name,
        personaDescription: persona.description,
        messages,
      })) {
        if (chunk.$type === "content") {
          setMessages((prev) => {
            const updated = [...prev];
            updated[updated.length - 1] = {
              ...updated[updated.length - 1],
              content: updated[updated.length - 1].content + chunk.content,
            };
            return updated;
          });
        } else if (chunk.$type === "usage") {
          setTokenUsage({
            inputTokens: chunk.inputTokens,
            outputTokens: chunk.outputTokens,
            totalTokens: chunk.totalTokens,
          });
        }
      }
    } catch (error) {
      console.error("Error compacting conversation:", error);
    } finally {
      setIsCompacting(false);
    }
  };

  return (
    <div {...props}>
      <ChatMessageList
        messages={messages}
        personaName={persona.name}
        onDeleteMessage={handleMessageDeleted}
        onEditMessage={handleMessageEdit}
        onRetryMessage={handleMessageRetry}
        isGenerating={isGenerating}
        className="mb-2"
      />

      <FooterContent>
        {model.contextWindow && tokenUsage && (
          <ContextUsageBar
            tokenUsage={tokenUsage}
            contextWindow={model.contextWindow}
          />
        )}
        <div className="flex items-center gap-2 p-4 border-t">
          <ChatInput
            inputText={inputText}
            isGenerating={isGenerating}
            isEditing={editingMessageIndex !== null}
            onInputChange={setInputText}
            onNewMessage={handleNewMessage}
            onUpdateMessage={handleUpdateMessage}
            onCancelEdit={handleCancelEdit}
            className="flex-1"
          />
        </div>
      </FooterContent>

      <ConversationConfigSheet
        open={isConfigOpen}
        onOpenChange={setIsConfigOpen}
        persona={persona}
        model={model}
        activeTraits={activeTraits}
        conversationName={conversationName}
        conversationIsPublic={conversationIsPublic}
        onRename={onRename}
        onDelete={onDelete}
        onVisibilityChange={onVisibilityChange}
        tokenUsage={tokenUsage}
        contextWindow={model.contextWindow}
        isCompacting={isCompacting}
        onCompact={messages.length > 0 ? handleCompact : undefined}
        onPersonaChange={(p) => {
          onPersonaChange?.(p);
          if (conversationId && p.id) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({ personaId: p.id }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
        onModelChange={(m) => {
          onModelChange?.(m);
          if (conversationId && m.id) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({ modelConfigId: m.id }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
        onTraitsChange={(traits) => {
          onTraitsChange?.(traits);
          if (conversationId) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({
                  traitIds: traits
                    .filter((t) => t.id && !t.isEphemeral)
                    .map((t) => t.id!),
                }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
      />
    </div>
  );
}
