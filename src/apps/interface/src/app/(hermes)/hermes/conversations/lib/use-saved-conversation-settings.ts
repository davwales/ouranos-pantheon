"use client";

import {
  type LoraFormInput,
  type ModelFormInput,
  type PersonaFormInput,
  type TraitFormInput,
} from "@/app/(hermes)/hermes/types";
import { type ReasoningEffort, type SavedConversation } from "@/lib/api/hermes";
import { useMemo, useState } from "react";
import {
  toModelFormInput,
  toPersonaFormInput,
  toTraitFormInputs,
} from "./saved-conversation-inputs";

/**
 * The chat settings of a resumed conversation: each starts from the saved value and follows the
 * user's local change once one is made.
 */
export function useSavedConversationSettings(
  saved: SavedConversation | undefined,
) {
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
  const [folderId, setFolderId] = useState<string | null | undefined>(
    undefined,
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

  return {
    persona: resolvedPersona,
    model: resolvedModel,
    traits: resolvedTraits,
    loras: resolvedLoras,
    loraIds: resolvedLoraIds,
    isPublic: isPublic ?? saved?.isPublic,
    reasoningEffort:
      reasoningEffort !== undefined
        ? reasoningEffort
        : (saved?.reasoningEffort ?? null),
    toolSetId: toolSetId !== undefined ? toolSetId : (saved?.toolSetId ?? null),
    folderId: folderId !== undefined ? folderId : (saved?.folderId ?? null),
    setPersona,
    setModel,
    setActiveTraits,
    setActiveLoras,
    setIsPublic,
    setReasoningEffort,
    setToolSetId,
    setFolderId,
  };
}
