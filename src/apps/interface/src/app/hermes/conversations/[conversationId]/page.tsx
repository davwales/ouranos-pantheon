"use client";

import ChatInterfaceView from "@/app/hermes/components/chat_interface_view";
import {
  ModelFormInput,
  PersonaFormInput,
  TraitFormInput,
} from "@/app/hermes/types";
import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import { useParams, useRouter } from "next/navigation";
import { useMemo, useState } from "react";

export default function ResumeConversationPage() {
  const { conversationId } = useParams<{ conversationId: string }>();
  const router = useRouter();
  const [state] = useApi(
    () => hermesApi.getConversation(conversationId),
    [conversationId],
  );
  const [savedConversationId, setSavedConversationId] =
    useState<string>(conversationId);
  const [conversationName, setConversationName] = useState<
    string | undefined
  >();
  const [persona, setPersona] = useState<PersonaFormInput | undefined>();
  const [model, setModel] = useState<ModelFormInput | undefined>();
  const [activeTraits, setActiveTraits] = useState<
    TraitFormInput[] | undefined
  >();
  const [isPublic, setIsPublic] = useState<boolean | undefined>();

  const saved = state.data;

  const resolvedPersona = useMemo<PersonaFormInput | undefined>(() => {
    if (persona) return persona;
    if (!saved) return undefined;
    return {
      id: saved.persona.id,
      name: saved.persona.name,
      description: saved.persona.description,
      personality: saved.persona.personality,
      scenario: saved.persona.scenario,
      isDefault: false,
      isPublic: false,
    };
  }, [persona, saved]);

  const resolvedModel = useMemo<ModelFormInput | undefined>(() => {
    if (model) return model;
    if (!saved) return undefined;
    return {
      id: saved.model.id,
      name: saved.model.name,
      modelIdentifier: saved.model.modelIdentifier,
      systemPrompt: saved.model.systemPrompt,
      temperature: saved.model.temperature,
      maxTokens: saved.model.maxTokens,
      repeatPenalty: saved.model.repeatPenalty,
      contextWindow: saved.model.contextWindow,
      isDefault: false,
      isPublic: false,
    };
  }, [model, saved]);

  const resolvedTraits = useMemo<TraitFormInput[]>(() => {
    if (activeTraits) return activeTraits;
    return (
      saved?.traits.map((t) => ({
        id: t.id,
        name: t.name,
        content: t.content,
        isPublic: false,
        isEphemeral: false,
      })) ?? []
    );
  }, [activeTraits, saved]);

  if (state.status === "loading") {
    return <div className="m-4">Loading...</div>;
  }

  if (
    state.status === "error" ||
    !saved ||
    !resolvedPersona ||
    !resolvedModel
  ) {
    return <div className="m-4">Failed to load conversation.</div>;
  }

  return (
    <ChatInterfaceView
      persona={resolvedPersona}
      model={resolvedModel}
      activeTraits={resolvedTraits}
      onPersonaChange={setPersona}
      onModelChange={setModel}
      onTraitsChange={setActiveTraits}
      initialMessages={saved.messages}
      initialTokenUsage={saved.tokenUsage}
      conversationId={savedConversationId}
      conversationName={conversationName ?? saved.name}
      conversationIsPublic={isPublic ?? saved.isPublic}
      onConversationSaved={(id, name) => {
        setSavedConversationId(id);
        setConversationName(name);
      }}
      onRename={async (name) => {
        await hermesApi.updateConversation(savedConversationId, {
          name,
          personaId: resolvedPersona.id!,
          modelConfigId: resolvedModel.id!,
          traitIds: resolvedTraits
            .filter((t) => t.id && !t.isEphemeral)
            .map((t) => t.id!),
          messages: saved.messages,
          isPublic: isPublic ?? saved.isPublic,
        });
        setConversationName(name);
      }}
      onDelete={async () => {
        await hermesApi.deleteConversation(savedConversationId);
        router.push("/hermes/conversations");
      }}
      onVisibilityChange={async (value) => {
        await hermesApi.updateConversation(savedConversationId, {
          name: conversationName ?? saved.name,
          personaId: resolvedPersona.id!,
          modelConfigId: resolvedModel.id!,
          traitIds: resolvedTraits
            .filter((t) => t.id && !t.isEphemeral)
            .map((t) => t.id!),
          messages: saved.messages,
          isPublic: value,
        });
        setIsPublic(value);
      }}
    />
  );
}
