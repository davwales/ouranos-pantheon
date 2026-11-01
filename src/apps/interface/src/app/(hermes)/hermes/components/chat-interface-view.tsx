"use client";

import { FooterContent } from "@/components/shared/footer";
import { useNavBarActions } from "@/components/shared/nav-bar-actions-context";
import ChatInput from "@/app/(hermes)/hermes/components/chat-input";
import ChatMessageList from "@/app/(hermes)/hermes/components/chat-message-list";
import { ContextUsageBar } from "@/app/(hermes)/hermes/components/context-usage-bar";
import { ConversationConfigSheet } from "@/app/(hermes)/hermes/components/conversation-config-sheet";
import { toImagePart } from "@/app/(hermes)/hermes/components/message-parts";
import {
  EDIT_IMAGE_TOOL,
  toolSetEnables,
} from "@/app/(hermes)/hermes/components/tool-set-constants";
import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import {
  type ImageActions,
  ImageActionsContext,
} from "@/components/shared/ai-assistant/image-actions-context";
import { useApi } from "@/hooks/use-api";
import {
  type LoraFormInput,
  ModelFormInput,
  PersonaFormInput,
  TraitFormInput,
} from "@/app/(hermes)/hermes/types";
import {
  hermesApi,
  hermesImagePath,
  MessageInput,
  type ReasoningEffort,
  Role,
  type UpdateConversationInput,
} from "@/lib/api/hermes";
import { toMessageInput, useMessageController } from "@/hooks/use-message-controller";
import { Bookmark, SlidersHorizontal } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";

export default function ChatInterfaceView({
  persona,
  model,
  activeTraits,
  activeLoras,
  reasoningEffort = null,
  toolSetId = null,
  onPersonaChange,
  onModelChange,
  onTraitsChange,
  onLorasChange,
  onReasoningEffortChange,
  onToolSetChange,
  conversationId,
  conversationName,
  conversationIsPublic = true,
  conversationFolderId = null,
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
  activeLoras: LoraFormInput[];
  reasoningEffort?: ReasoningEffort | null;
  toolSetId?: string | null;
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
  onTraitsChange?: (traits: TraitFormInput[]) => void;
  onLorasChange?: (loras: LoraFormInput[]) => void;
  onReasoningEffortChange?: (effort: ReasoningEffort | null) => void;
  onToolSetChange?: (toolSetId: string | null) => void;
  conversationId?: string;
  conversationName?: string;
  conversationIsPublic?: boolean;
  conversationFolderId?: string | null;
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
  const [attachedImage, setAttachedImage] = useState<AssistantChatImagePart | null>(null);
  const [isConfigOpen, setIsConfigOpen] = useState(false);
  const [composedSystemPrompt, setComposedSystemPrompt] = useState<
    string | null
  >(null);
  const { setActions, clearActions } = useNavBarActions();

  const {
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
  } = useMessageController({
    initialMessages: initialMessages ?? [],
    initialTokenUsage: initialTokenUsage ?? null,
    model,
    reasoningEffort,
    toolSetId,
    persona,
    activeTraits,
    activeLoras,
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
      loraIds?: string[];
      reasoningEffort?: ReasoningEffort | null;
      toolSetId?: string | null;
    }): UpdateConversationInput => ({
      name: conversationName ?? "New Conversation",
      personaId: overrides.personaId ?? persona.id!,
      modelConfigId: overrides.modelConfigId ?? model.id!,
      traitIds:
        overrides.traitIds ??
        activeTraits.filter((t) => t.id && !t.isEphemeral).map((t) => t.id!),
      loraIds: overrides.loraIds ?? loraIdsOf(activeLoras),
      messages: overrides.messages,
      isPublic: conversationIsPublic,
      folderId: conversationFolderId,
      reasoningEffort:
        overrides.reasoningEffort !== undefined
          ? overrides.reasoningEffort
          : reasoningEffort,
      toolSetId:
        overrides.toolSetId !== undefined ? overrides.toolSetId : toolSetId,
    }),
    [
      conversationName,
      conversationIsPublic,
      conversationFolderId,
      reasoningEffort,
      toolSetId,
      persona.id,
      model.id,
      activeTraits,
      activeLoras,
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
        loraIds: loraIdsOf(activeLoras),
        messages: messages.map(toMessageInput),
        inputTokenCount: tokenUsage?.inputTokens ?? null,
        outputTokenCount: tokenUsage?.outputTokens ?? null,
        totalTokenCount: tokenUsage?.totalTokens ?? null,
        reasoningEffort,
        toolSetId,
      });
      onConversationSaved?.(result.id, result.name);
    } catch (error) {
      console.error("Error saving conversation:", error);
    }
  }, [
    persona.id,
    model.id,
    activeTraits,
    activeLoras,
    messages,
    tokenUsage,
    reasoningEffort,
    toolSetId,
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

  const [toolSetsState] = useApi(() => hermesApi.getAllToolSets());
  const canEditImages = toolSetEnables(
    toolSetsState.data?.find((set) => set.id === toolSetId),
    EDIT_IMAGE_TOOL,
  );

  const imageActions = useMemo((): ImageActions => {
    const canAct = canEditImages && !isGenerating && !isCompacting;
    return {
      loadSource,
      onEdit: canAct ? setAttachedImage : undefined,
      onMoreLikeThis: canAct
        ? (image) => {
            setAttachedImage(null);
            void addUserMessage("More like this", toImagePart(image));
          }
        : undefined,
    };
  }, [canEditImages, isGenerating, isCompacting, addUserMessage]);

  const handleUpdateMessage = () => {
    if (editingMessageIndex === null || !inputText.trim()) return;
    updateMessageContent(editingMessageIndex, inputText);
    setEditingMessageIndex(null);
    setInputText("");
  };

  const handleNewMessage = async () => {
    const text = inputText;
    const image = attachedImage;
    setInputText("");
    setAttachedImage(null);
    await addUserMessage(text, image ? toImagePart(image) : undefined);
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
      <ImageActionsContext.Provider value={imageActions}>
        <ChatMessageList
          messages={messages}
          streamingParts={streamingParts}
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
      </ImageActionsContext.Provider>
      {generationError && (
        <p role="alert" className="mx-auto w-full max-w-3xl px-2 text-sm text-destructive">
          {generationError}
        </p>
      )}

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
            onStop={stop}
            attachment={canEditImages ? attachedImage : null}
            onRemoveAttachment={() => setAttachedImage(null)}
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
        activeLoras={activeLoras}
        reasoningEffort={reasoningEffort}
        toolSetId={toolSetId}
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
          onToolSetChange?.(m.defaultToolSetId);
          if (conversationId && m.id) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({
                  modelConfigId: m.id,
                  toolSetId: m.defaultToolSetId,
                }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
        onReasoningEffortChange={(effort) => {
          onReasoningEffortChange?.(effort);
          if (conversationId) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({ reasoningEffort: effort }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
        onToolSetChange={(id) => {
          onToolSetChange?.(id);
          if (conversationId) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({ toolSetId: id }),
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
        onLorasChange={(loras) => {
          onLorasChange?.(loras);
          if (conversationId) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({ loraIds: loraIdsOf(loras) }),
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

async function loadSource(image: AssistantChatImagePart): Promise<AssistantChatImagePart | null> {
  const details = await hermesApi.getImageDetails(image.imageId);
  if (!details.sourceImageId) {
    return null;
  }

  const source = await hermesApi.getImageDetails(details.sourceImageId);
  return {
    kind: "image",
    id: source.id,
    callId: "",
    imageId: source.id,
    url: hermesImagePath(source.id),
    width: source.width,
    height: source.height,
    altText: source.prompt,
  };
}

function loraIdsOf(loras: LoraFormInput[]): string[] {
  return loras.flatMap((l) => (l.id ? [l.id] : []));
}
