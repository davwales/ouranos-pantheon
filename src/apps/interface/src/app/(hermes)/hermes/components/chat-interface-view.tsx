"use client";

import { FooterContent } from "@/components/shared/footer";
import { useNavBarActions } from "@/components/shared/nav-bar-actions-context";
import ChatInput from "@/app/(hermes)/hermes/components/chat-input";
import ChatMessageList from "@/app/(hermes)/hermes/components/chat-message-list";
import { ContextUsageBar } from "@/app/(hermes)/hermes/components/context-usage-bar";
import { ConversationConfigSheet } from "@/app/(hermes)/hermes/components/conversation-config-sheet";
import {
  ModelFormInput,
  PersonaFormInput,
  TraitFormInput,
} from "@/app/(hermes)/hermes/types";
import { hermesApi, MessageInput, Role } from "@/lib/api/hermes";
import { useMessageController } from "@/hooks/use-message-controller";
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
  const [inputText, setInputText] = useState("");
  const [editingMessageIndex, setEditingMessageIndex] = useState<number | null>(
    null,
  );
  const [isConfigOpen, setIsConfigOpen] = useState(false);
  const [composedSystemPrompt, setComposedSystemPrompt] = useState<
    string | null
  >(null);
  const { setActions, clearActions } = useNavBarActions();

  const {
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
  } = useMessageController({
    initialMessages: initialMessages ?? [],
    initialTokenUsage: initialTokenUsage ?? null,
    model,
    persona,
    activeTraits,
    conversationId,
    onSystemPromptChange: setComposedSystemPrompt,
    onMessagesPersist: conversationId
      ? (updatedMessages: MessageInput[]) => {
          hermesApi
            .updateConversation(
              conversationId,
              buildUpdatePayload({ messages: updatedMessages }),
            )
            .catch((err) =>
              console.error("Failed to update conversation:", err),
            );
        }
      : undefined,
  });

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

  const handleUpdateMessage = () => {
    if (editingMessageIndex === null || !inputText.trim()) return;
    updateMessageContent(editingMessageIndex, inputText);
    setEditingMessageIndex(null);
    setInputText("");
  };

  const handleNewMessage = async () => {
    const text = inputText;
    setInputText("");
    await addUserMessage(text);
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
    deleteMessage(index);
  };

  const handleMessageRetry = async (index: number) => {
    await retryMessage(index);
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
        isCompacting={isCompacting}
        compactionError={compactionError}
        onRetryCompact={() => {
          const lastSummaryIndex = messages.findLastIndex(
            (m) => m.role === Role.Summary,
          );
          if (lastSummaryIndex !== -1) {
            retryMessage(lastSummaryIndex);
          }
        }}
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
        composedSystemPrompt={composedSystemPrompt}
        onCompact={messages.length > 0 ? () => compactMessages(messages) : undefined}
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
