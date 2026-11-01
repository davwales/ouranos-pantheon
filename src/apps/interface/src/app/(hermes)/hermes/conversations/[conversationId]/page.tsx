"use client";

import ChatInterfaceView from "@/app/(hermes)/hermes/components/chat-interface-view";
import {
  type LoraFormInput,
  type ModelFormInput,
  type PersonaFormInput,
  type TraitFormInput,
} from "@/app/(hermes)/hermes/types";
import { ConversationHeader } from "@/app/(hermes)/hermes/conversations/components/conversation-header";
import { usePublicMode } from "@/app/(hermes)/hermes/components/public-mode-context";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useApi } from "@/hooks/use-api";
import {
  hermesApi,
  type ReasoningEffort,
  type UpdateConversationInput,
} from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useCallback, useMemo, useState } from "react";
import { buildFolderTree, flattenFolders } from "../lib/folder-utils";
import {
  toModelFormInput,
  toPersonaFormInput,
  toTraitFormInputs,
} from "../lib/saved-conversation-inputs";
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
  const [folderId, setFolderId] = useState<string | null | undefined>(
    undefined,
  );
  const [persona, setPersona] = useState<PersonaFormInput | undefined>();
  const [model, setModel] = useState<ModelFormInput | undefined>();
  const [activeTraits, setActiveTraits] = useState<
    TraitFormInput[] | undefined
  >();
  const [activeLoras, setActiveLoras] = useState<
    LoraFormInput[] | undefined
  >();
  const [isPublic, setIsPublic] = useState<boolean | undefined>();
  const [reasoningEffort, setReasoningEffort] = useState<
    ReasoningEffort | null | undefined
  >(undefined);
  const [toolSetId, setToolSetId] = useState<string | null | undefined>(
    undefined,
  );

  const [state] = useApi(
    () => hermesApi.getConversation(conversationId),
    [conversationId],
  );
  useBreadcrumbLabel(conversationId, state.data?.name, state.status === "error");
  const [foldersState] = useApi(() => hermesApi.getAllFolders());

  const saved = state.data;

  const resolvedReasoningEffort =
    reasoningEffort !== undefined ? reasoningEffort : saved?.reasoningEffort ?? null;
  const resolvedToolSetId =
    toolSetId !== undefined ? toolSetId : saved?.toolSetId ?? null;

  const resolvedFolderId = useMemo(
    () => (folderId !== undefined ? folderId : saved?.folderId ?? null),
    [folderId, saved?.folderId],
  );

  const folderTree = useMemo(
    () => buildFolderTree(foldersState.data ?? []),
    [foldersState.data],
  );

  const allFolders = useMemo(
    () => flattenFolders(folderTree),
    [folderTree],
  );

  const resolvedPersona = useMemo<PersonaFormInput | undefined>(() => {
    if (persona) return persona;
    if (!saved) return undefined;
    return toPersonaFormInput(saved.persona);
  }, [persona, saved]);

  const resolvedModel = useMemo<ModelFormInput | undefined>(() => {
    if (model) return model;
    if (!saved) return undefined;
    return toModelFormInput(saved.model);
  }, [model, saved]);

  const resolvedTraits = useMemo<TraitFormInput[]>(() => {
    if (activeTraits) return activeTraits;
    return saved ? toTraitFormInputs(saved.traits) : [];
  }, [activeTraits, saved]);

  const resolvedLoras = useMemo<LoraFormInput[]>(() => {
    if (activeLoras) return activeLoras;
    return saved?.loras.map((l) => ({ ...l, isPublic: false })) ?? [];
  }, [activeLoras, saved]);

  const resolvedLoraIds = useMemo(
    () => resolvedLoras.flatMap((l) => (l.id ? [l.id] : [])),
    [resolvedLoras],
  );

  /** Saves the conversation's current settings, with the given fields overridden. */
  const updateConversation = useCallback(
    async (overrides: Partial<UpdateConversationInput>) => {
      if (!saved || !resolvedPersona || !resolvedModel) return;

      await hermesApi.updateConversation(savedConversationId, {
        name: conversationName ?? saved.name,
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
      conversationName,
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
    [saved, resolvedPersona, resolvedModel, updateConversation],
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
        name={conversationName ?? saved.name}
        folders={allFolders}
        currentFolderId={resolvedFolderId}
        onMove={isPublicMode ? undefined : handleMoveToFolder}
      />

      <ChatInterfaceView
        persona={resolvedPersona}
        model={resolvedModel}
        activeTraits={resolvedTraits}
        activeLoras={resolvedLoras}
        reasoningEffort={resolvedReasoningEffort}
        toolSetId={resolvedToolSetId}
        onPersonaChange={setPersona}
        onModelChange={setModel}
        onTraitsChange={setActiveTraits}
        onLorasChange={setActiveLoras}
        onReasoningEffortChange={setReasoningEffort}
        onToolSetChange={setToolSetId}
        initialMessages={saved.messages}
        initialTokenUsage={saved.tokenUsage}
        // Public mode continues a saved conversation as an unsaved copy: without its id, nothing
        // the visitor does is written back.
        conversationId={isPublicMode ? undefined : savedConversationId}
        conversationName={conversationName ?? saved.name}
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
