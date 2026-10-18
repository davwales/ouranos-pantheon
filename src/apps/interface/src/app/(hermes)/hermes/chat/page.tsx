"use client";

import SelectConfigView from "@/app/(hermes)/hermes/chat/views/select-config-view";
import ChatInterfaceView from "@/app/(hermes)/hermes/components/chat-interface-view";
import {
  type LoraFormInput,
  ModelFormInput,
  PersonaFormInput,
  TraitFormInput,
} from "@/app/(hermes)/hermes/types";
import { hermesApi, type ReasoningEffort } from "@/lib/api/hermes";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ChatMessageSkeleton } from "@/components/shared/skeletons/chat-message-skeleton";

type ConversationState = "loading" | "setup" | "chat";

export default function Conversation() {
  const router = useRouter();
  const [persona, setPersona] = useState<PersonaFormInput>();
  const [model, setModel] = useState<ModelFormInput>();
  const [activeTraits, setActiveTraits] = useState<TraitFormInput[]>([]);
  const [activeLoras, setActiveLoras] = useState<LoraFormInput[]>([]);
  const [reasoningEffort, setReasoningEffort] = useState<ReasoningEffort | null>(null);
  const [toolSetId, setToolSetId] = useState<string | null>(null);
  const [conversationState, setConversationState] =
    useState<ConversationState>("loading");
  const [conversationId, setConversationId] = useState<string | undefined>();
  const [conversationName, setConversationName] = useState<
    string | undefined
  >();

  const handleModelChange = (selected: ModelFormInput | undefined) => {
    setModel(selected);
    setToolSetId(selected?.defaultToolSetId ?? null);
  };

  useEffect(() => {
    const loadDefaults = async () => {
      try {
        const [personas, models] = await Promise.all([
          hermesApi.getAllPersonas(),
          hermesApi.getAllModels(),
        ]);

        const defaultPersona = personas.find((p) => p.isDefault);
        const defaultModel = models.find((m) => m.isDefault);

        if (defaultPersona) {
          setPersona({ ...defaultPersona });
        }
        if (defaultModel) {
          handleModelChange({ ...defaultModel });
        }

        if (defaultPersona && defaultModel) {
          setConversationState("chat");
        } else {
          setConversationState("setup");
        }
      } catch {
        setConversationState("setup");
      }
    };

    loadDefaults();
  }, []);

  const handleBeginConversation = () => {
    if (persona && model) {
      setConversationState("chat");
    }
  };

  if (conversationState === "loading") {
    return (
      <div className="m-4">
        <ChatMessageSkeleton pairCount={1} />
      </div>
    );
  }

  if (conversationState === "chat" && persona && model) {
    return (
      <ChatInterfaceView
        persona={persona}
        model={model}
        activeTraits={activeTraits}
        activeLoras={activeLoras}
        reasoningEffort={reasoningEffort}
        toolSetId={toolSetId}
        onPersonaChange={setPersona}
        onModelChange={setModel}
        onTraitsChange={setActiveTraits}
        onLorasChange={setActiveLoras}
        onReasoningEffortChange={setReasoningEffort}
        onToolSetChange={setToolSetId}
        conversationId={conversationId}
        conversationName={conversationName}
        onConversationSaved={(id, name) => {
          setConversationId(id);
          setConversationName(name);
          router.replace(`/hermes/conversations/${id}`);
        }}
      />
    );
  }

  return (
    <div className="m-4">
      <SelectConfigView
        persona={persona}
        setPersona={setPersona}
        model={model}
        setModel={handleModelChange}
        onBegin={handleBeginConversation}
        beginDisabled={!persona || !model}
      />
    </div>
  );
}
