"use client";

import ChatInterfaceView from "@/app/(hermes)/hermes/components/chat-interface-view";
import { ConversationHeader } from "@/app/(hermes)/hermes/conversations/components/conversation-header";
import { usePublicMode } from "@/app/(hermes)/hermes/components/use-public-mode";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useApi } from "@/hooks/use-api";
import { hermesApi, type UpdateConversationInput } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useCallback, useMemo, useState } from "react";
import { buildFolderTree, flattenFolders } from "../lib/folder-utils";
import { useGeneratedConversationName } from "../lib/use-generated-conversation-name";
import { useSavedConversationSettings } from "../lib/use-saved-conversation-settings";
import { ChatMessageSkeleton } from "@/components/shared/skeletons/chat-message-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";

export default function ResumeConversationPage() {
  const { conversationId } = useParams<{ conversationId: string }>();
  const router = useRouter();
  const isPublicMode = usePublicMode();
  const [savedConversationId, setSavedConversationId] =
    useState<string>(conversationId);
  const [conversationName, setConversationName] = useState<
    string | undefined
  >();

  const [state] = useApi(
    () => hermesApi.getConversation(conversationId),
    [conversationId],
  );
  const [foldersState] = useApi(() => hermesApi.getAllFolders());

  const saved = state.data;
  const settings = useSavedConversationSettings(saved);
  const {
    persona: resolvedPersona,
    model: resolvedModel,
    traits: resolvedTraits,
    loraIds: resolvedLoraIds,
    reasoningEffort: resolvedReasoningEffort,
    toolSetId: resolvedToolSetId,
    folderId: resolvedFolderId,
    isPublic,
    setFolderId,
    setIsPublic,
  } = settings;
  const generatedName = useGeneratedConversationName(
    savedConversationId,
    conversationName ?? saved?.name,
    saved?.createdAt,
  );
  const displayName = conversationName ?? generatedName ?? saved?.name;
  useBreadcrumbLabel(conversationId, displayName, state.status === "error");

  const folderTree = useMemo(
    () => buildFolderTree(foldersState.data ?? []),
    [foldersState.data],
  );

  const allFolders = useMemo(
    () => flattenFolders(folderTree),
    [folderTree],
  );

  /** Saves the conversation's current settings, with the given fields overridden. */
  const updateConversation = useCallback(
    async (overrides: Partial<UpdateConversationInput>) => {
      if (!saved || !resolvedPersona || !resolvedModel) return;

      await hermesApi.updateConversation(savedConversationId, {
        personaId: resolvedPersona.id!,
        modelConfigId: resolvedModel.id!,
        traitIds: resolvedTraits
          .filter((t) => t.id && !t.isEphemeral)
          .map((t) => t.id!),
        loraIds: resolvedLoraIds,
        isPublic: isPublic ?? saved.isPublic,
        folderId: resolvedFolderId,
        reasoningEffort: resolvedReasoningEffort,
        toolSetId: resolvedToolSetId,
        ...overrides,
      });
    },
    [
      saved,
      resolvedPersona,
      resolvedModel,
      savedConversationId,
      resolvedTraits,
      resolvedLoraIds,
      isPublic,
      resolvedFolderId,
      resolvedReasoningEffort,
      resolvedToolSetId,
    ],
  );

  const handleMoveToFolder = useCallback(
    async (targetFolderId: string | null) => {
      if (!saved || !resolvedPersona || !resolvedModel) return;

      await updateConversation({ folderId: targetFolderId });
      setFolderId(targetFolderId);
    },
    [saved, resolvedPersona, resolvedModel, updateConversation, setFolderId],
  );

  if (state.status === "loading") {
    return (
      <div className="m-4">
        <ChatMessageSkeleton pairCount={2} />
      </div>
    );
  }

  if (
    state.status === "error" ||
    !saved ||
    !resolvedPersona ||
    !resolvedModel
  ) {
    return (
      <NotFoundCard
        title="Conversation not found"
        message="This conversation doesn't exist or has been removed."
        backHref="/hermes/conversations"
        backLabel="Back to Conversations"
      />
    );
  }

  return (
    <>
      <ConversationHeader
        name={displayName ?? saved.name}
        folders={allFolders}
        currentFolderId={resolvedFolderId}
        onMove={isPublicMode ? undefined : handleMoveToFolder}
      />

      <ChatInterfaceView
        persona={resolvedPersona}
        model={resolvedModel}
        activeTraits={resolvedTraits}
        activeLoras={settings.loras}
        reasoningEffort={resolvedReasoningEffort}
        toolSetId={resolvedToolSetId}
        onPersonaChange={settings.setPersona}
        onModelChange={settings.setModel}
        onTraitsChange={settings.setActiveTraits}
        onLorasChange={settings.setActiveLoras}
        onReasoningEffortChange={settings.setReasoningEffort}
        onToolSetChange={settings.setToolSetId}
        initialMessages={saved.messages}
        initialTokenUsage={saved.tokenUsage}
        // Public mode continues a saved conversation as an unsaved copy: without its id, nothing
        // the visitor does is written back.
        conversationId={isPublicMode ? undefined : savedConversationId}
        conversationName={displayName ?? saved.name}
        conversationIsPublic={isPublic ?? saved.isPublic}
        conversationFolderId={resolvedFolderId}
        onConversationSaved={(id, name) => {
          setSavedConversationId(id);
          setConversationName(name);
        }}
        onRename={
          isPublicMode
            ? undefined
            : async (name) => {
                await updateConversation({ name });
                setConversationName(name);
              }
        }
        onDelete={
          isPublicMode
            ? undefined
            : async () => {
                await hermesApi.deleteConversation(savedConversationId);
                router.push("/hermes/conversations");
              }
        }
        onVisibilityChange={
          isPublicMode
            ? undefined
            : async (value) => {
                await updateConversation({ isPublic: value });
                setIsPublic(value);
              }
        }
      />
    </>
  );
}
